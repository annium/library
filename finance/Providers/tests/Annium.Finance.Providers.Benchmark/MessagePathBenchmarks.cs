using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures;
using Annium.Logging.Shared;
using Annium.Serialization.Abstractions;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using UsdFutures = Annium.Finance.Providers.Crypto.Binance.UsdFutures;

namespace Annium.Finance.Providers.Benchmark;

/// <summary>
/// Measures what one market message costs from bytes to subscriber, with no exchange and no socket.
/// </summary>
/// <remarks>
/// <para>
/// The connector-throughput spec closed its live figures with a caveat: they were taken <em>at rest</em>,
/// with no ticker stream flowing, so the per-message path had been read rather than measured. These rows
/// are that measurement, in the form the spec asked for — an offline run over a payload the provider tests
/// already carry.
/// </para>
/// <para>
/// Three rows, because the message crosses three costs that are paid by different threads and changed by
/// different code. <c>Deserialize</c> is what the socket's read thread pays to turn bytes into a domain
/// value. <c>Write</c> is what that same thread pays to hand the value off. <c>WriteAndDeliver</c> is the
/// whole path including the pump that carries it to subscribers — the only row that includes the thread
/// handoff, and the only one whose number is not simply the sum of the others.
/// </para>
/// <para>
/// The serializer is the one the provider registers, resolved by the key production uses, so what is
/// measured is the real converter chain rather than a plain <c>JsonSerializer</c> call that happens to
/// produce the same type.
/// </para>
/// <para>
/// The payload is the recorded book-ticker envelope from <c>StreamDataTests</c>, re-flowed onto one line.
/// The fixture is indented for a human reader; the exchange sends no whitespace, and measuring the parser
/// skipping eighty spaces that never arrive would overstate every row that touches it.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class MessagePathBenchmarks
{
    /// <summary>
    /// How many messages a delivery row pushes through per invocation.
    /// </summary>
    /// <remarks>
    /// Delivery is asynchronous: the write returns before the pump has carried anything, so a single
    /// message cannot be timed - the measurement would be the write alone plus whatever the wait costs.
    /// A batch amortizes the wait over enough messages that what remains is the per-message cost, and
    /// <c>OperationsPerInvoke</c> reports it that way.
    /// </remarks>
    private const int BatchSize = 1000;

    /// <summary>
    /// The recorded book-ticker envelope, as the exchange sends it: one line, no whitespace.
    /// </summary>
    private const string Payload =
        @"{""stream"":""btcusdt@bookTicker"",""data"":{""u"":17242169,""s"":""BTCUSDT"",""b"":""9548.1"",""B"":""52"",""a"":""9548.5"",""A"":""11""}}";

    /// <summary>The payload as bytes, encoded once so encoding is not charged to the rows.</summary>
    private ReadOnlyMemory<byte> _raw;

    /// <summary>The serializer the provider registers for the book-ticker stream.</summary>
    private ISerializer<ReadOnlyMemory<byte>> _serializer = null!;

    /// <summary>A ticker built once, so the delivery rows measure delivery and not construction.</summary>
    private InstrumentTicker _ticker = null!;

    /// <summary>A connector delivering through the channel and pump, as a live connector does.</summary>
    private BenchmarkConnector _buffered = null!;

    /// <summary>A connector delivering on the writer's thread, as the backtest's replay connectors do.</summary>
    private BenchmarkConnector _inline = null!;

    /// <summary>
    /// Builds the container, resolves the registered serializer, and brings both connectors to the state
    /// in which they deliver.
    /// </summary>
    /// <returns>A task representing the setup.</returns>
    [GlobalSetup]
    public async Task SetupAsync()
    {
        _raw = Encoding.UTF8.GetBytes(Payload);
        _ticker = new InstrumentTicker("BTCUSDT", 9548.1m, 9548.5m);

        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack().Register(
                (container, _) =>
                {
                    container.AddRuntime(GetType().Assembly);
                    container.AddTime().WithRealTime().SetDefault();
                    container.AddLogging();
                    container.AddMapper();
                    container.AddFinanceProviders().WithBinanceUsdFutures();
                }
            )
        );

        var sp = await builder.BuildAsync(CancellationToken.None);

        var serializerKey = SerializerKey.Create(UsdFutures.Constants.InstrumentTickerKey, "application/json");
        _serializer = sp.ResolveKeyed<ISerializer<ReadOnlyMemory<byte>>>(serializerKey);

        _buffered = await BenchmarkConnector.StartAsync(sp, ConnectorDelivery.Buffered);
        _inline = await BenchmarkConnector.StartAsync(sp, ConnectorDelivery.Inline);
    }

    /// <summary>Disposes both connectors.</summary>
    /// <returns>A task representing the teardown.</returns>
    [GlobalCleanup]
    public async Task CleanupAsync()
    {
        await _buffered.DisposeAsync();
        await _inline.DisposeAsync();
    }

    /// <summary>
    /// Bytes to domain value, through the converters the provider registers.
    /// </summary>
    /// <returns>The deserialized envelope.</returns>
    [Benchmark]
    public StreamData<InstrumentTicker>? Deserialize() => _serializer.Deserialize<StreamData<InstrumentTicker>?>(_raw);

    /// <summary>
    /// The handoff alone, buffered: what the socket's read thread pays before returning to read again.
    /// </summary>
    [Benchmark]
    public void Write_Buffered() => _buffered.Ticker(_ticker);

    /// <summary>
    /// The handoff alone, inline: delivery happens on this thread, before the call returns.
    /// </summary>
    [Benchmark]
    public void Write_Inline() => _inline.Ticker(_ticker);

    /// <summary>
    /// The whole path, buffered: written on this thread, delivered to a subscriber on the pump's.
    /// </summary>
    /// <returns>A task that completes when every message of the batch has reached the subscriber.</returns>
    [Benchmark(OperationsPerInvoke = BatchSize)]
    public Task WriteAndDeliverBufferedAsync() => _buffered.WriteAndAwaitAsync(_ticker, BatchSize);

    /// <summary>
    /// The whole path, inline: delivery is part of the write, so this differs from <see cref="Write_Inline"/>
    /// only by the counting the subscriber does.
    /// </summary>
    /// <returns>A task that completes when every message of the batch has reached the subscriber.</returns>
    [Benchmark(OperationsPerInvoke = BatchSize)]
    public Task WriteAndDeliverInlineAsync() => _inline.WriteAndAwaitAsync(_ticker, BatchSize);
}
