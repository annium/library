using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Connectors.Market;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Core.Tests.Market;

/// <summary>
/// Pins the sharing contract of <see cref="IMarketConnectorFactory.CreatePooled" />: one connector per
/// settings, alive for as long as any lease on it is.
/// </summary>
public class PooledMarketConnectorTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PooledMarketConnectorTests" /> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public PooledMarketConnectorTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.Add<ScopeCapture>().AsSelf().Singleton();
            container.Add<FakeInstanceFactory>().AsKeyed<IMarketConnectorInstanceFactory>("fake").Scoped();
            container.Add<FakeInstanceFactory>().AsKeyed<IMarketConnectorInstanceFactory>("other").Scoped();
        });
    }

    /// <summary>
    /// Tests that two leases on the same settings are served by one connector, while Create keeps
    /// building its own.
    /// </summary>
    [Fact]
    public void SameSettings_ShareOneConnector_WhileCreateDoesNot()
    {
        // arrange
        var factory = Get<IMarketConnectorFactory>();
        var settings = new MarketSettings { Provider = "fake" };

        // act
        var first = factory.CreatePooled(settings);
        var second = factory.CreatePooled(settings);
        var own = factory.Create(settings);

        // assert - the leases resolve to one connector, and the directly created one is not it
        first.Instruments.IsEqual(second.Instruments);
        ReferenceEquals(first, second).IsFalse("each lease is its own handle onto the shared connector");
        Get<ScopeCapture>().Created.Is(2, "one connector for the shared leases, one for Create");
        own.IsNotDefault();
    }

    /// <summary>
    /// Tests that different settings are not shared with each other.
    /// </summary>
    [Fact]
    public void DifferentSettings_DoNotShare()
    {
        // arrange
        var factory = Get<IMarketConnectorFactory>();

        // act
        factory.CreatePooled(new MarketSettings { Provider = "fake" });
        factory.CreatePooled(new MarketSettings { Provider = "other" });

        // assert
        Get<ScopeCapture>().Created.Is(2, "settings are the sharing key");
    }

    /// <summary>
    /// Tests that the shared connector survives until the last lease is returned, and that returning a
    /// lease twice does not release it twice.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task SharedConnector_LivesUntilTheLastLeaseIsReturned()
    {
        // arrange
        var capture = Get<ScopeCapture>();
        var factory = Get<IMarketConnectorFactory>();
        var settings = new MarketSettings { Provider = "fake" };
        var first = factory.CreatePooled(settings);
        var second = factory.CreatePooled(settings);

        // act - one holder lets go, twice over
        await first.DisposeAsync();
        await first.DisposeAsync();

        // assert - the other holder still has a connector
        capture.Disposed.Is(0, "a returned lease must not take a connector another holder is using");

        // act - and now the last one
        await second.DisposeAsync();

        // assert
        capture.Disposed.Is(1, "the connector goes once nobody holds it");
    }

    /// <summary>
    /// Tests that a fresh lease after the last one was returned builds a new connector rather than
    /// handing back the disposed one.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task LeaseAfterRelease_BuildsAgain()
    {
        // arrange
        var capture = Get<ScopeCapture>();
        var factory = Get<IMarketConnectorFactory>();
        var settings = new MarketSettings { Provider = "fake" };

        // act
        await factory.CreatePooled(settings).DisposeAsync();
        factory.CreatePooled(settings);

        // assert
        capture.Created.Is(2, "the pool must not hand back a connector it has torn down");
    }

    /// <summary>Counts what the fake factory built and what was torn down.</summary>
    private sealed class ScopeCapture
    {
        /// <summary>Gets how many connectors the instance factory has built.</summary>
        public int Created { get; private set; }

        /// <summary>Gets how many of them have been disposed.</summary>
        public int Disposed { get; private set; }

        /// <summary>Records that a connector was built.</summary>
        public void RecordCreated() => Created++;

        /// <summary>Records that a connector was disposed.</summary>
        public void RecordDisposed() => Disposed++;
    }

    /// <summary>
    /// Stands in for a provider's own connector factory, counting what it builds.
    /// </summary>
    /// <param name="capture">The counters to report through.</param>
    private sealed class FakeInstanceFactory(ScopeCapture capture) : IMarketConnectorInstanceFactory
    {
        /// <summary>
        /// Counts the call and returns a connector that owns nothing.
        /// </summary>
        /// <param name="settings">Ignored.</param>
        /// <param name="disposable">Ignored.</param>
        /// <returns>A connector that does nothing but report its disposal.</returns>
        public IMarketConnector Create(MarketSettings settings, AsyncDisposableBox disposable)
        {
            capture.RecordCreated();

            return new StubConnector(capture);
        }
    }

    /// <summary>A connector that does nothing beyond reporting that it was disposed.</summary>
    /// <param name="capture">The counters to report through.</param>
    private sealed class StubConnector(ScopeCapture capture) : IMarketConnector
    {
        /// <summary>Gets the connector's status; unused by these tests.</summary>
        public ConnectorStatus Status => ConnectorStatus.Connected;

        /// <summary>Gets the connector's resources; unused by these tests.</summary>
        public IReadOnlyCollection<ResourceModel> Resources => [];

        /// <summary>Gets the connector's instruments; unused by these tests.</summary>
        public IReadOnlyCollection<InstrumentModel> Instruments => [];

        /// <summary>Gets the connector's tickers; unused by these tests.</summary>
        public IObservable<InstrumentTicker> Tickers => System.Reactive.Linq.Observable.Empty<InstrumentTicker>();

        /// <summary>Raised when the status changes; unused by these tests.</summary>
        public event Action<ConnectorStatus> OnStatusChanged = delegate { };

        /// <summary>Raised on failure; unused by these tests.</summary>
        public event Action<ConnectorError> OnError = delegate { };

        /// <summary>Raised on sync; unused by these tests.</summary>
        public event Func<
            MarketSettings,
            IReadOnlyCollection<ResourceModel>,
            IReadOnlyCollection<InstrumentModel>,
            Task
        > OnSync = delegate
        {
            return Task.CompletedTask;
        };

        /// <summary>Signals a sync; unused by these tests.</summary>
        public void Sync() => OnStatusChanged(ConnectorStatus.Connected);

        /// <summary>Subscribes to tickers; unused by these tests.</summary>
        /// <param name="symbols">Ignored.</param>
        public void SubscribeTickers(IReadOnlyCollection<string> symbols) { }

        /// <summary>Unsubscribes from tickers; unused by these tests.</summary>
        /// <param name="symbols">Ignored.</param>
        public void UnsubscribeTickers(IReadOnlyCollection<string> symbols) => OnError(new ConnectorError("unused"));

        /// <summary>Records the disposal.</summary>
        /// <returns>A completed task.</returns>
        public ValueTask DisposeAsync()
        {
            capture.RecordDisposed();

            return ValueTask.CompletedTask;
        }
    }
}
