using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Connectors.Market;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Core.Market;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Finance.Providers.Core.Tests.Market;

/// <summary>
/// Pins what the market connector factory hands to a provider's own instance factory. Until now nothing
/// exercised this path offline at all: its only callers are the connector test bases, and every test built on
/// those talks to the live exchange and is skipped.
/// </summary>
public class MarketConnectorFactoryTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarketConnectorFactoryTests"/> class, registering a
    /// stand-in instance factory so the real factory has a provider to build for.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper used to capture test logs.</param>
    public MarketConnectorFactoryTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.Add<CreationLog>().AsSelf().Singleton();
            container.Add<FakeInstanceFactory>().AsKeyed<IMarketConnectorInstanceFactory>("fake").Transient();
        });
    }

    /// <summary>
    /// Every connector gets a status monitor of its own. The monitor is the one thing that has to be
    /// per-connector - it aggregates that connector and the components it is built from - and it used to be
    /// per-connector only as a side effect of a DI scope each.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task EachConnector_GetsItsOwnMonitor()
    {
        // arrange
        var log = Get<CreationLog>();
        var factory = Get<IMarketConnectorFactory>();
        var settings = new MarketSettings { Provider = "fake" };

        // act
        await using var first = factory.Create(settings);
        await using var second = factory.Create(settings);

        // assert
        log.Creations.Has(2);
        log.Creations[0].Monitor.IsNotDefault();
        log.Creations[1].Monitor.IsNotDefault();
        ReferenceEquals(log.Creations[0].Monitor, log.Creations[1].Monitor)
            .IsFalse("two connectors sharing one monitor resolve each other's status as their own");
    }

    /// <summary>
    /// The factory builds through whatever provider it was resolved from, rather than opening a scope of its
    /// own. That is what lets a caller running work in its own scope - a backtest, say - have the connector's
    /// components come from that scope.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Factory_BuildsThroughTheProviderItWasResolvedFrom()
    {
        // arrange
        var log = Get<CreationLog>();
        var settings = new MarketSettings { Provider = "fake" };
        await using var scope = Get<IServiceProvider>().CreateAsyncScope();

        // act
        var factory = scope.ServiceProvider.Resolve<IMarketConnectorFactory>();
        await using var connector = factory.Create(settings);

        // assert
        log.Creations.Has(1);
        log.Creations[0].Sp.Is(scope.ServiceProvider, "the connector was not built through the caller's scope");
    }

    /// <summary>What an instance factory was handed for one connector.</summary>
    /// <param name="Sp">The provider the instance factory itself was resolved from.</param>
    /// <param name="Monitor">The monitor the connector was told to report into.</param>
    private sealed record Creation(IServiceProvider Sp, IStatusMonitor Monitor);

    /// <summary>Collects what the instance factory saw, in creation order.</summary>
    private sealed class CreationLog
    {
        /// <summary>Gets the creations recorded so far.</summary>
        public List<Creation> Creations { get; } = new();
    }

    /// <summary>
    /// Stands in for a provider's own connector factory, recording what it is handed.
    /// </summary>
    /// <param name="sp">The provider this factory was resolved from.</param>
    /// <param name="log">The log creations are recorded in.</param>
    private sealed class FakeInstanceFactory(IServiceProvider sp, CreationLog log) : IMarketConnectorInstanceFactory
    {
        /// <summary>
        /// Records the call and returns a connector that owns nothing.
        /// </summary>
        /// <param name="settings">Ignored.</param>
        /// <param name="monitor">The monitor the caller supplied, recorded.</param>
        /// <param name="disposable">Ignored.</param>
        /// <returns>A connector that does nothing.</returns>
        public IMarketConnector Create(MarketSettings settings, IStatusMonitor monitor, AsyncDisposableBox disposable)
        {
            log.Creations.Add(new Creation(sp, monitor));

            return new StubConnector();
        }
    }

    /// <summary>A connector that does nothing, so the test observes only the factory's own behaviour.</summary>
    private sealed class StubConnector : IMarketConnector
    {
        /// <summary>Gets the connector's status; unused by this test.</summary>
        public ConnectorStatus Status => ConnectorStatus.Connected;

        /// <summary>Gets the connector's resources; unused by this test.</summary>
        public IReadOnlyCollection<ResourceModel> Resources => [];

        /// <summary>Gets the connector's instruments; unused by this test.</summary>
        public IReadOnlyCollection<InstrumentModel> Instruments => [];

        /// <summary>Gets the connector's ticker stream; unused by this test.</summary>
        public IObservable<InstrumentTicker> Tickers => Observable.Empty<InstrumentTicker>();

        /// <summary>Raised on status change; unused by this test.</summary>
        public event Action<ConnectorStatus> OnStatusChanged = delegate { };

        /// <summary>Raised on error; unused by this test.</summary>
        public event Action<ConnectorError> OnError = delegate { };

        /// <summary>Raised on sync; unused by this test.</summary>
        public event Func<
            MarketSettings,
            IReadOnlyCollection<ResourceModel>,
            IReadOnlyCollection<InstrumentModel>,
            Task
        > OnSync = delegate
        {
            return Task.CompletedTask;
        };

        /// <summary>Does nothing; unused by this test.</summary>
        public void Sync() => OnStatusChanged(ConnectorStatus.Connected);

        /// <summary>Does nothing; unused by this test.</summary>
        /// <param name="symbols">Ignored.</param>
        public void SubscribeTickers(IReadOnlyCollection<string> symbols) => _ = OnSync(new MarketSettings(), [], []);

        /// <summary>Does nothing; unused by this test.</summary>
        /// <param name="symbols">Ignored.</param>
        public void UnsubscribeTickers(IReadOnlyCollection<string> symbols) => OnError(new ConnectorError("unused"));

        /// <summary>Does nothing.</summary>
        /// <returns>A completed task.</returns>
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
