using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Abstractions.Domain.User.Operations;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Services;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Net.Servers.Web;
using Annium.Serialization.Abstractions;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User;

/// <summary>
/// Builds a USD-M futures user connector by hand, against a local HTTP server and fakes for everything the
/// connector only talks to through an interface, so its command and ingestion paths can be driven offline.
/// </summary>
/// <remarks>
/// <para>
/// The live block reaches the happy path of four commands and nothing else. Everything a real exchange will
/// not produce on demand - a refusal, a query that does not build, a disconnected connector, a stream event
/// of a chosen shape - is only reachable here.
/// </para>
/// <para>
/// Two things make this possible without DI. The connector binds itself to the monitor as connected and
/// then adopts the monitor's aggregate, so with a fresh monitor and nothing else bound it is connected the
/// moment it is built. And the signature service is faked, because resolving the real one starts a server
/// time poller against the exchange from its constructor - the one way an offline test reaches the network.
/// </para>
/// </remarks>
public abstract class UserConnectorOfflineTestBase : ProvidersTestBase
{
    /// <summary>
    /// How long any connector test may run before xUnit fails it, in milliseconds.
    /// </summary>
    private const int TimeoutMs = 30_000;

    /// <summary>
    /// The deadline every test in this hierarchy runs under.
    /// </summary>
    protected const int Timeout = TimeoutMs;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectorOfflineTestBase"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    protected UserConnectorOfflineTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the USD-M futures provider, which is what puts the keyed request factories and serializers
    /// the connector resolves into the container.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceUsdFutures();
    }

    /// <summary>
    /// Builds a connector pointed at the given local server, with fakes the test can drive and observe.
    /// </summary>
    /// <param name="server">The local server answering the connector's HTTP calls.</param>
    /// <param name="parts">The fakes the connector was built with.</param>
    /// <param name="monitor">The monitor to report into; a fresh one, leaving the connector connected, when null.</param>
    /// <returns>The connector under test.</returns>
    protected IUserConnector CreateConnector(IServer server, out ConnectorParts parts, StatusMonitor? monitor = null)
    {
        var sp = Get<IServiceProvider>();
        var logger = Get<ILogger>();
        var reload = new CompositeLoaderConfig(1, 2, 5, 0, 0);
        var config = new UserConfig
        {
            Provider = Constants.Provider,
            Key = "some_key",
            Secret = "some_secret",
            HttpApi = server.HttpUri(),
            WsApi = new Uri("ws://unused"),
            ListenKeyUriPath = "/unused/",
            ListenKey = new ListenKeyConfiguration(1000, 1000),
            ReloadContext = reload,
            ReloadOrders = reload,
            ReloadTrades = reload,
        };

        var statusMonitor = monitor ?? new StatusMonitor(logger);
        var built = new ConnectorParts(statusMonitor);

        var connector = new UserConnector(
            config,
            built.Provider,
            new QueryProcessor(),
            new StubSignatureService(),
            sp.ResolveHttpRequestFactory(Constants.SetLeverageKey),
            sp.ResolveHttpRequestFactory(Constants.InitOrderKey),
            sp.ResolveHttpRequestFactory(Constants.ModifyOrderKey),
            sp.ResolveHttpRequestFactory(Constants.CancelOrderKey),
            sp.ResolveHttpRequestFactory(Constants.CancelAllOrdersKey),
            new StubRateLimiter(),
            built.ContextLoader,
            built.OrdersLoader,
            built.TradesLoader,
            built.Stream,
            sp.ResolveSerializer<ReadOnlyMemory<byte>>(Constants.OrderUpdateKey, MediaTypeNames.Application.Json),
            statusMonitor.CreateReporter(),
            statusMonitor,
            Disposable.AsyncBox(logger),
            logger
        );

        parts = built;

        return connector;
    }

    /// <summary>
    /// The fakes a connector was built with, for the test to drive and to assert on.
    /// </summary>
    protected sealed class ConnectorParts
    {
        /// <summary>Gets the monitor the connector reports into.</summary>
        public StatusMonitor Monitor { get; }

        /// <summary>Gets the provider the connector loads through.</summary>
        public StubUserProvider Provider { get; } = new();

        /// <summary>Gets the account context loader.</summary>
        public StubCompositeLoader<UserContext> ContextLoader { get; } = new();

        /// <summary>Gets the open orders loader.</summary>
        public StubCompositeLoader<IReadOnlyCollection<OrderModel>> OrdersLoader { get; } = new();

        /// <summary>Gets the trades loader.</summary>
        public StubKeyedLoader TradesLoader { get; } = new();

        /// <summary>Gets the user stream the test pushes messages through.</summary>
        public StubUserStream Stream { get; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorParts"/> class.
        /// </summary>
        /// <param name="monitor">The monitor the connector reports into.</param>
        public ConnectorParts(StatusMonitor monitor)
        {
            Monitor = monitor;
        }
    }

    /// <summary>
    /// A user provider that loads nothing, since the connector's own loaders are faked too.
    /// </summary>
    protected sealed class StubUserProvider : IUserProvider
    {
        /// <summary>Loads the account context.</summary>
        /// <returns>An empty context.</returns>
        public Task<UserResult<UserContext?>> LoadContextAsync() =>
            Task.FromResult(UserResult.New(UserOperationStatus.Ok, default(UserContext)));

        /// <summary>Loads open orders.</summary>
        /// <returns>No orders.</returns>
        public Task<UserResult<IReadOnlyCollection<OrderModel>?>> LoadOpenOrdersAsync() =>
            Task.FromResult(UserResult.New(UserOperationStatus.Ok, (IReadOnlyCollection<OrderModel>?)[]));

        /// <summary>Loads orders for a symbol.</summary>
        /// <param name="symbol">The symbol to load for.</param>
        /// <param name="since">The point in time to load from.</param>
        /// <returns>No orders.</returns>
        public Task<UserResult<IReadOnlyCollection<OrderModel>?>> LoadOrdersAsync(string symbol, long? since) =>
            Task.FromResult(UserResult.New(UserOperationStatus.Ok, (IReadOnlyCollection<OrderModel>?)[]));

        /// <summary>Loads trades for a symbol.</summary>
        /// <param name="symbol">The symbol to load for.</param>
        /// <param name="since">The point in time to load from.</param>
        /// <returns>No trades.</returns>
        public Task<UserResult<IReadOnlyCollection<TradeModel>?>> LoadTradesAsync(string symbol, long? since) =>
            Task.FromResult(UserResult.New(UserOperationStatus.Ok, (IReadOnlyCollection<TradeModel>?)[]));
    }

    /// <summary>
    /// A composite loader the test drives by hand, recording what the connector asked it for.
    /// </summary>
    /// <typeparam name="T">The type of data the loader carries.</typeparam>
    protected sealed class StubCompositeLoader<T> : ICompositeLoader<T>
    {
        /// <summary>Raised with loaded data.</summary>
        public event Action<T> OnData = delegate { };

        /// <summary>Gets the channel every reload request is written into.</summary>
        public Channel<int> Requests { get; } = Channel.CreateUnbounded<int>();

        /// <summary>Gets the number of times the loader was started.</summary>
        public int Starts => Volatile.Read(ref _starts);

        /// <summary>Gets the number of times the loader was stopped.</summary>
        public int Stops => Volatile.Read(ref _stops);

        /// <summary>How many times the loader was started.</summary>
        private int _starts;

        /// <summary>How many times the loader was stopped.</summary>
        private int _stops;

        /// <summary>Hands the connector a load result.</summary>
        /// <param name="data">The data to hand over.</param>
        public void Emit(T data) => OnData(data);

        /// <summary>Records a start.</summary>
        /// <param name="reportStatus">Whether the loader reports status; ignored here.</param>
        public void Start(bool reportStatus) => Interlocked.Increment(ref _starts);

        /// <summary>Records a stop.</summary>
        public void Stop() => Interlocked.Increment(ref _stops);

        /// <summary>Records a reload request.</summary>
        public void Request() => Requests.Writer.TryWrite(0);

        /// <summary>Does nothing; the loader holds no resources here.</summary>
        public void Dispose() { }
    }

    /// <summary>
    /// A keyed loader the test drives by hand, recording the keys the connector asked for.
    /// </summary>
    protected sealed class StubKeyedLoader : IKeyedLoader<string, long, IReadOnlyCollection<TradeModel>>
    {
        /// <summary>Raised with loaded trades.</summary>
        public event Action<string, long, IReadOnlyCollection<TradeModel>> OnData = delegate { };

        /// <summary>Gets the channel every requested key is written into.</summary>
        public Channel<string> Requests { get; } = Channel.CreateUnbounded<string>();

        /// <summary>Hands the connector trades for a key.</summary>
        /// <param name="key">The key the trades are for.</param>
        /// <param name="context">The context the trades were loaded with.</param>
        /// <param name="trades">The trades to hand over.</param>
        public void Emit(string key, long context, IReadOnlyCollection<TradeModel> trades) =>
            OnData(key, context, trades);

        /// <summary>Records a request for a key.</summary>
        /// <param name="key">The key requested.</param>
        public void Request(string key) => Requests.Writer.TryWrite(key);

        /// <summary>Does nothing; the loader holds no resources here.</summary>
        public void Dispose() { }
    }

    /// <summary>
    /// A user stream the test pushes raw messages through.
    /// </summary>
    protected sealed class StubUserStream : IUserStream
    {
        /// <summary>Raised when the stream connects.</summary>
        public event Action OnConnected = delegate { };

        /// <summary>Raised when the stream disconnects.</summary>
        public event Action OnDisconnected = delegate { };

        /// <summary>Raised for every message received.</summary>
        public event Action<ReadOnlyMemory<byte>> OnMessage = delegate { };

        /// <summary>Tells the connector the stream connected.</summary>
        public void Connect() => OnConnected();

        /// <summary>Tells the connector the stream disconnected.</summary>
        public void Disconnect() => OnDisconnected();

        /// <summary>Pushes a raw message at the connector.</summary>
        /// <param name="payload">The raw payload to push.</param>
        public void Push(ReadOnlyMemory<byte> payload) => OnMessage(payload);

        /// <summary>Does nothing; the stream holds no resources here.</summary>
        public void Dispose() { }
    }

    /// <summary>
    /// A rate limiter that never delays and never blocks.
    /// </summary>
    protected sealed class StubRateLimiter : IRateLimiter
    {
        /// <summary>Allows every request.</summary>
        /// <returns>Always true.</returns>
        public bool CanExecute() => true;

        /// <summary>Ignores the reported limit.</summary>
        /// <param name="limit">The limit reported.</param>
        public void UpdateLimit(int limit) { }

        /// <summary>Ignores the reported weight.</summary>
        /// <param name="weight">The weight reported.</param>
        public void UsedWeight(int weight) { }

        /// <summary>Ignores the block.</summary>
        /// <param name="duration">The duration to block for.</param>
        public void Block(TimeSpan duration) { }

        /// <summary>Does nothing; the limiter holds no resources here.</summary>
        public void Dispose() { }
    }

    /// <summary>
    /// A signature service that signs nothing.
    /// </summary>
    /// <remarks>
    /// Not merely convenience: resolving the real one starts a server time poller against the exchange from
    /// its constructor, which is how an offline test quietly reaches the network.
    /// </remarks>
    protected sealed class StubSignatureService : ISignatureService
    {
        /// <summary>Gets a fixed server time.</summary>
        public long ServerTime => 1_700_000_000_000;

        /// <summary>Gets the api key.</summary>
        /// <returns>The api key.</returns>
        public string GetKey() => "some_key";

        /// <summary>Signs the given data.</summary>
        /// <param name="data">The data to sign.</param>
        /// <returns>The signature.</returns>
        public string GetSignature(string data) => "signature";
    }

    /// <summary>
    /// Records what a local server was asked for, so a test can assert on the request the connector built.
    /// </summary>
    protected sealed class RequestLog
    {
        /// <summary>Gets every request the server answered, in order.</summary>
        public ConcurrentQueue<RecordedRequest> Requests { get; } = new();

        /// <summary>Gets the channel every answered request is written into.</summary>
        public Channel<RecordedRequest> Answered { get; } = Channel.CreateUnbounded<RecordedRequest>();

        /// <summary>Records a request.</summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="path">The path requested.</param>
        /// <param name="query">The query string.</param>
        public void Add(string method, string path, string query)
        {
            var recorded = new RecordedRequest(method, path, query);
            Requests.Enqueue(recorded);
            Answered.Writer.TryWrite(recorded);
        }
    }

    /// <summary>
    /// One request a local server answered.
    /// </summary>
    /// <param name="Method">The HTTP method.</param>
    /// <param name="Path">The path requested.</param>
    /// <param name="Query">The query string, without its leading question mark.</param>
    protected sealed record RecordedRequest(string Method, string Path, string Query);
}
