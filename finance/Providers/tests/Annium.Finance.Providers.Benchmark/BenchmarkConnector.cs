using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Connectors.Market;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Abstractions.Domain.Market.Operations;
using Annium.Finance.Providers.Core.Market;
using Annium.Finance.Providers.Core.Shared;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Logging;
using NodaTime;

namespace Annium.Finance.Providers.Benchmark;

/// <summary>
/// A market connector with no provider behind it, used to measure what delivery costs.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MarketConnectorBase"/> keeps both the write and the channel protected, so the only way to
/// measure the real delivery path — rather than a channel built to look like it — is to inherit, which is
/// what the connector tests do for the same reason.
/// </para>
/// <para>
/// The sync in <see cref="StartAsync"/> is not ceremony. A connector's readers are subscribed by the sync
/// cycle, so before one has run nothing is pumping: values written go into the channel and stay there.
/// Measuring delivery without it would have measured a write to a queue nobody reads, and the buffered row
/// would have looked like the cheapest path in the file.
/// </para>
/// </remarks>
internal sealed class BenchmarkConnector : MarketConnectorBase
{
    /// <summary>How many messages the subscriber has seen since the current batch was armed.</summary>
    private long _received;

    /// <summary>How many the current batch is waiting for, or -1 when no batch is armed.</summary>
    private long _target = -1;

    /// <summary>Completed when the armed batch has fully arrived.</summary>
    private TaskCompletionSource _batch = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Initializes a new instance of the <see cref="BenchmarkConnector"/> class.
    /// </summary>
    /// <param name="settings">The settings to construct the connector with.</param>
    /// <param name="provider">The provider backing the connector, which is never called.</param>
    /// <param name="reporter">The status reporter to bind to.</param>
    /// <param name="monitor">The status monitor to observe.</param>
    /// <param name="delivery">How the connector hands written tickers to its subscribers.</param>
    /// <param name="logger">The logger to use.</param>
    private BenchmarkConnector(
        MarketSettings settings,
        IMarketProvider provider,
        IStatusReporter reporter,
        IStatusMonitor monitor,
        ConnectorDelivery delivery,
        ILogger logger
    )
        : base(settings, provider, reporter, monitor, delivery, Annium.Disposable.AsyncBox(logger), logger) { }

    /// <summary>
    /// Builds a connector with the given delivery and brings it to the state in which it delivers.
    /// </summary>
    /// <param name="sp">The service provider to take a logger from.</param>
    /// <param name="delivery">The delivery mode to measure.</param>
    /// <returns>A connector whose subscribers are receiving.</returns>
    public static async Task<BenchmarkConnector> StartAsync(IServiceProvider sp, ConnectorDelivery delivery)
    {
        var logger = sp.Resolve<ILogger>();
        var monitor = new StatusMonitor(logger);
        var connector = new BenchmarkConnector(
            new MarketSettings { Provider = "benchmark" },
            new NullMarketProvider(),
            monitor.CreateReporter(),
            monitor,
            delivery,
            logger
        );

        connector.Tickers.Subscribe(connector.Count);
        connector.ScheduleSync([], []);

        // wait until a probe actually comes out the other end, rather than for a status that says it should
        await connector.WriteAndAwaitAsync(new InstrumentTicker("PROBE", 1m, 1m), 1);

        return connector;
    }

    /// <summary>
    /// Writes one ticker, exposing the protected write the socket thread calls.
    /// </summary>
    /// <param name="ticker">The ticker to write.</param>
    public void Ticker(InstrumentTicker ticker) => Write(ticker);

    /// <summary>
    /// Writes a batch and returns when the subscriber has seen all of it.
    /// </summary>
    /// <param name="ticker">The ticker to write repeatedly.</param>
    /// <param name="count">How many to write.</param>
    /// <returns>A task completing when the batch has been delivered.</returns>
    public async Task WriteAndAwaitAsync(InstrumentTicker ticker, int count)
    {
        _batch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Interlocked.Exchange(ref _received, 0);
        Interlocked.Exchange(ref _target, count);

        for (var i = 0; i < count; i++)
        {
            Write(ticker);
        }

        // the batch is completed by the subscriber, on the pump's thread - which is exactly the handoff
        // being measured, not foreign work this call has no business awaiting
#pragma warning disable VSTHRD003
        await _batch.Task;
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Counts an arrival and completes the batch when it is full.
    /// </summary>
    /// <param name="ticker">The delivered ticker, which is not inspected.</param>
    private void Count(InstrumentTicker ticker)
    {
        _ = ticker;

        if (Interlocked.Increment(ref _received) >= Interlocked.Read(ref _target))
        {
            _batch.TrySetResult();
        }
    }

    /// <summary>
    /// A provider that is never called: the connector under measurement loads nothing.
    /// </summary>
    /// <remarks>
    /// Every member throws rather than returning an empty result. A benchmark that quietly loaded nothing
    /// would report the cost of a connector that is not the one production runs, and the throw says so at
    /// the first call instead.
    /// </remarks>
    private sealed class NullMarketProvider : IMarketProvider
    {
        /// <summary>Never called.</summary>
        /// <returns>Never returns.</returns>
        public Task<MarketResult<MarketContext?>> LoadContextAsync() => throw new NotSupportedException();

        /// <summary>Never called.</summary>
        /// <param name="instrument">Unused.</param>
        /// <param name="start">Unused.</param>
        /// <param name="end">Unused.</param>
        /// <param name="ct">Unused.</param>
        /// <returns>Never returns.</returns>
        public IAsyncEnumerable<MarketResult<IReadOnlyCollection<CandleModel>?>> LoadCandlesAsync(
            string instrument,
            Instant start,
            Instant end,
            CancellationToken ct
        ) => throw new NotSupportedException();
    }
}
