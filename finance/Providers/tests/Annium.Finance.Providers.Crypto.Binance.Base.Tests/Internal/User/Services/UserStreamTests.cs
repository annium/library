using System;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Crypto.Binance.Base.Internal.User.Services;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Tests.Internal.User.Services;

/// <summary>
/// Pins how <see cref="UserStream"/> follows the listen key: it opens the socket on a key, carries that key
/// in the URL, closes on a reset, asks for a new key when the exchange drops it, and forwards what arrives.
/// </summary>
/// <remarks>
/// The stream sits between two things a live test cannot steer - the resolver's events and the exchange's
/// socket - so every property here is reachable only with both of them under the test's control.
/// </remarks>
public class UserStreamTests : ProvidersTestBase
{
    /// <summary>
    /// How long any test here may run before xUnit fails it, in milliseconds.
    /// </summary>
    private const int TimeoutMs = 30_000;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserStreamTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public UserStreamTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A fetched key opens the socket, and the key goes in the URL.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task KeyFetched_ConnectsWithTheKeyInTheUrl()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        var resolver = new TestListenKeyResolver();
        using var stream = CreateStream(server, resolver, monitor);

        // act
        resolver.Fetch("abc");

        // assert
        var connection = await server.WaitConnectionAsync(ct);
        connection.RequestUri.AbsolutePath.Is("/ws/abc");
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
    }

    /// <summary>
    /// A reset key closes the socket, rather than leaving one open against a key the exchange has dropped.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task KeyReset_ClosesTheConnection()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        var resolver = new TestListenKeyResolver();
        using var stream = CreateStream(server, resolver, monitor);
        resolver.Fetch("abc");
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        resolver.Reset();

        // assert
        await connection.WhenClosed.WaitAsync(ct);
    }

    /// <summary>
    /// A connection the exchange drops makes the stream ask for a new key, because the old one is spent.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Drop_RequestsANewKey()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        var resolver = new TestListenKeyResolver();
        using var stream = CreateStream(server, resolver, monitor);
        var disconnects = Channel.CreateUnbounded<int>();
        stream.OnDisconnected += () => disconnects.Writer.TryWrite(0);
        resolver.Fetch("abc");
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        connection.Drop();

        // assert
        await resolver.Requests.Reader.ReadAsync(ct);
        await disconnects.Reader.ReadAsync(ct);
    }

    /// <summary>
    /// What arrives on the socket is forwarded verbatim.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Message_IsForwarded()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        var resolver = new TestListenKeyResolver();
        using var stream = CreateStream(server, resolver, monitor);
        var messages = Channel.CreateUnbounded<string>();
        stream.OnMessage += raw => messages.Writer.TryWrite(Encoding.UTF8.GetString(raw.Span));
        resolver.Fetch("abc");
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        await connection.SendAsync("""{"e":"ORDER_TRADE_UPDATE"}""", ct);

        // assert
        (await messages.Reader.ReadAsync(ct)).Is("""{"e":"ORDER_TRADE_UPDATE"}""");
    }

    /// <summary>
    /// Disposing closes the socket, leaves the monitor, and raises nothing on the way out.
    /// </summary>
    /// <remarks>
    /// The disconnect that teardown itself causes is not news to anyone - and reporting it would run
    /// against a reporter about to be unbound. A disposed stream is gone, not disconnected.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Dispose_ClosesConnectionAndLeavesTheMonitor()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());

        // a second target, so that the monitor still has one after the stream leaves
        var peer = monitor.CreateReporter();
        peer.Bind(new object(), ConnectorStatus.Connected);

        var resolver = new TestListenKeyResolver();
        var stream = CreateStream(server, resolver, monitor);
        var disconnects = Channel.CreateUnbounded<int>();
        stream.OnDisconnected += () => disconnects.Writer.TryWrite(0);
        resolver.Fetch("abc");
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        // VSTHRD103: the stream's teardown is synchronous, and disposing it is the act under test
#pragma warning disable VSTHRD103
        stream.Dispose();
#pragma warning restore VSTHRD103

        // assert
        await connection.WhenClosed.WaitAsync(ct);
        monitor.Status.Is(ConnectorStatus.Connected);
        disconnects.Reader.Count.Is(0);
        resolver.Requests.Reader.Count.Is(0);
    }

    /// <summary>
    /// Builds a stream pointed at the given local server, driven by the given resolver.
    /// </summary>
    /// <param name="server">The local server the stream connects to.</param>
    /// <param name="resolver">The resolver the stream follows.</param>
    /// <param name="monitor">The monitor the stream reports into.</param>
    /// <returns>The stream under test.</returns>
    private UserStream CreateStream(TestWebSocketServer server, IListenKeyResolver resolver, StatusMonitor monitor)
    {
        var config = new TestUserConfig
        {
            Provider = "test",
            Key = "key",
            Secret = "secret",
            HttpApi = new Uri("http://localhost"),
            WsApi = server.Uri,
            ListenKeyUriPath = "ws/",
            ListenKey = new ListenKeyConfiguration(50, 50),
        };

        return new UserStream(config, resolver, monitor.CreateReporter(), Get<ILogger>());
    }

    /// <summary>
    /// A listen key resolver the test drives by hand, recording what the stream asked it for.
    /// </summary>
    private sealed class TestListenKeyResolver : IListenKeyResolver
    {
        /// <summary>Raised when a listen key is fetched.</summary>
        public event Action<string> OnListenKeyFetched = delegate { };

        /// <summary>Raised when the current listen key is invalidated.</summary>
        public event Action OnListenKeyReset = () => { };

        /// <summary>Gets the channel every request for a new key is written into.</summary>
        public Channel<int> Requests { get; } = Channel.CreateUnbounded<int>();

        /// <summary>Hands the stream a key.</summary>
        /// <param name="key">The key to hand over.</param>
        public void Fetch(string key) => OnListenKeyFetched(key);

        /// <summary>Tells the stream its key is spent.</summary>
        public void Reset() => OnListenKeyReset();

        /// <summary>Records the stream's request for a new key.</summary>
        public void RequestNewListenKey() => Requests.Writer.TryWrite(0);

        /// <summary>Does nothing; the resolver holds no resources here.</summary>
        /// <returns>A completed value task.</returns>
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

/// <summary>
/// A user configuration pointed at a local server.
/// </summary>
file sealed record TestUserConfig : UserConfigBase;
