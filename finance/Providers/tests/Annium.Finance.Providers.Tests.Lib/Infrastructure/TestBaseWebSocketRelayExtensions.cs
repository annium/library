using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Servers.Sockets;
using Annium.Testing;

namespace Annium.Finance.Providers.Tests.Lib.Infrastructure;

/// <summary>
/// Runs a local relay in front of a real venue, so a live test can cut the connection on purpose.
/// </summary>
/// <remarks>
/// The counterpart of <see cref="TestBaseWebSocketServerExtensions"/> for the half it cannot reach. That
/// server answers instead of the venue, which pins how our code reacts to a drop and says nothing about
/// what the venue does when we come back. This relays to the venue and lets the test cut the wire.
/// </remarks>
public static class TestBaseWebSocketRelayExtensions
{
    /// <summary>
    /// Starts a relay that forwards to the given upstream endpoint.
    /// </summary>
    /// <param name="test">The test instance the relay resolves its services from.</param>
    /// <param name="upstream">The venue endpoint to relay to.</param>
    /// <returns>The running relay; dispose it to stop listening.</returns>
    public static TestWebSocketRelay RunWebSocketRelay(this TestBase test, Uri upstream)
    {
        var sp = test.Get<IServiceProvider>();

        return new TestWebSocketRelay(sp, upstream, 0, sp.Resolve<ILogger>());
    }

    /// <summary>
    /// Starts a relay on a port the caller already knows.
    /// </summary>
    /// <remarks>
    /// For the case the overload above cannot serve: routing a whole connector through the relay, where the
    /// connector is built by the container and configured while it is being built, so its endpoint has to be
    /// known before anything the relay needs exists. Reserving the port first inverts that — the address is
    /// decided by the test, the provider is configured with it, and the relay opens on it afterwards.
    /// </remarks>
    /// <param name="test">The test instance the relay resolves its services from.</param>
    /// <param name="upstream">The venue endpoint to relay to.</param>
    /// <param name="port">The port to listen on, as returned by <see cref="ReserveLocalPort"/>.</param>
    /// <returns>The running relay; dispose it to stop listening.</returns>
    public static TestWebSocketRelay RunWebSocketRelay(this TestBase test, Uri upstream, ushort port)
    {
        var sp = test.Get<IServiceProvider>();

        return new TestWebSocketRelay(sp, upstream, port, sp.Resolve<ILogger>());
    }

    /// <summary>
    /// Picks a free local port by opening a listener on any port and reading which one the system gave.
    /// </summary>
    /// <remarks>
    /// The listener is closed immediately, so what comes back is a port that was free a moment ago rather
    /// than one that is held — nothing else here can guarantee more, and the window is a few milliseconds on
    /// a loopback interface. Said plainly because a reserved-sounding name would suggest otherwise.
    /// </remarks>
    /// <returns>The port number.</returns>
    public static ushort ReserveLocalPort()
    {
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));

        return (ushort)((IPEndPoint)listener.LocalEndPoint.NotNull()).Port;
    }
}

/// <summary>
/// A byte-for-byte relay between a local client and a real venue.
/// </summary>
/// <remarks>
/// <para>
/// <b>It is not a WebSocket proxy, and that is the whole point.</b> A proxy that understood WebSocket
/// would terminate the protocol's own control frames: the venue's ping would be answered by the proxy's
/// stack and never reach the client, so the one property worth testing over a long connection - that the
/// client answers - would be masked by the instrument measuring it. This copies bytes and understands
/// nothing, so a ping is just bytes and arrives.
/// </para>
/// <para>
/// The one thing it does read is the opening HTTP upgrade, to rewrite the <c>Host</c> header: a client
/// pointed at a local port names that port, and a venue served by name will not accept a request
/// addressed to somewhere else. Everything after that first request is copied untouched.
/// </para>
/// <para>
/// The client speaks plaintext to the relay and the relay speaks TLS to the venue. That is not a
/// weakening of anything: the encrypted half is the half that leaves the machine, and the plaintext half
/// is a loopback socket the test owns. Terminating TLS is also what makes the relay possible at all -
/// a client that spoke TLS to the relay would be checking the venue's certificate against it.
/// </para>
/// </remarks>
public sealed class TestWebSocketRelay : IAsyncDisposable, ILogSubject
{
    /// <summary>
    /// Gets the logger this relay traces through.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the URI a client connects to instead of the venue.
    /// </summary>
    public Uri Uri => new($"ws://{_server.Host}:{_server.Port}{_upstream.PathAndQuery}");

    /// <summary>
    /// Gets the number of connections the relay has accepted, including ones already closed.
    /// </summary>
    public int AcceptedConnections => Volatile.Read(ref _acceptedConnections);

    /// <summary>
    /// The upstream endpoint being relayed to.
    /// </summary>
    private readonly Uri _upstream;

    /// <summary>
    /// The listening server.
    /// </summary>
    private readonly IServer _server;

    /// <summary>
    /// The client side of every live connection, so the test can cut them.
    /// </summary>
    private readonly List<Socket> _live = new();

    /// <summary>
    /// Guards <see cref="_live"/>.
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// How many connections have been accepted.
    /// </summary>
    private int _acceptedConnections;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestWebSocketRelay"/> class and starts listening.
    /// </summary>
    /// <param name="sp">The service provider the server is built from.</param>
    /// <param name="upstream">The venue endpoint to relay to.</param>
    /// <param name="port">The port to listen on, or zero to let the system pick one.</param>
    /// <param name="logger">The logger to trace through.</param>
    internal TestWebSocketRelay(IServiceProvider sp, Uri upstream, ushort port, ILogger logger)
    {
        Logger = logger;
        _upstream = upstream;
        _server = (port is 0 ? ServerBuilder.New(sp) : ServerBuilder.New(sp, port))
            .WithHandler(new RelayHandler(this))
            .Start()
            .NotNull();

        this.Trace("started relay at port {port}", _server.Port);
    }

    /// <summary>
    /// Cuts every live connection, as a network would.
    /// </summary>
    /// <remarks>
    /// The point of the whole class: a venue will not drop a connection to order, and a connector's
    /// recovery is unreachable without one that does.
    /// </remarks>
    public void Drop()
    {
        this.Trace("start");

        Socket[] live;
        lock (_locker)
        {
            live = _live.ToArray();
            _live.Clear();
        }

        foreach (var socket in live)
            try
            {
                socket.Close();
            }
            catch (Exception ex)
            {
                // a socket the other side has already closed throws here, which is the same outcome asked
                // for and not a failure of the test
                this.Trace<string>("drop of an already closed socket: {error}", ex.Message);
            }

        this.Trace<string>("dropped {count} connection(s)", live.Length.ToString());
    }

    /// <summary>
    /// Stops the relay and cuts anything still connected.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        Drop();
        await _server.DisposeAsync();

        this.Trace("done");
    }

    /// <summary>
    /// Relays one accepted connection to the venue, copying bytes in both directions until either side
    /// closes.
    /// </summary>
    /// <param name="client">The socket the client connected on.</param>
    /// <param name="ct">The server's cancellation token.</param>
    /// <returns>A task that completes when the connection closes.</returns>
    private async Task HandleAsync(Socket client, CancellationToken ct)
    {
        this.Trace("start");

        Interlocked.Increment(ref _acceptedConnections);

        lock (_locker)
            _live.Add(client);

        using var upstreamClient = new TcpClient();

        try
        {
            await upstreamClient.ConnectAsync(_upstream.Host, _upstream.Port, ct);

            await using var upstream = new SslStream(upstreamClient.GetStream(), leaveInnerStreamOpen: false);

            // authenticated by the venue's own host name rather than by whatever the client addressed, so
            // the certificate check is the real one
            await upstream.AuthenticateAsClientAsync(_upstream.Host);

            await using var downstream = new NetworkStream(client, ownsSocket: false);

            var request = await ReadUpgradeRequestAsync(downstream, ct);
            var rewritten = RewriteHost(request, _upstream.Host);
            await upstream.WriteAsync(Encoding.ASCII.GetBytes(rewritten), ct);
            await upstream.FlushAsync(ct);

            // and from here nothing is read for meaning: control frames, data frames and the close
            // handshake are all just bytes, which is what lets the client answer the venue's ping itself
            var toClient = CopyAsync(upstream, downstream, ct);
            var toVenue = CopyAsync(downstream, upstream, ct);

            await Task.WhenAny(toClient, toVenue);
        }
        catch (Exception ex)
        {
            // a cut connection lands here by design, and so does a venue that closed first
            this.Trace<string>("relay ended: {error}", ex.Message);
        }
        finally
        {
            lock (_locker)
                _live.Remove(client);

            client.Close();
        }

        this.Trace("done");
    }

    /// <summary>
    /// Reads the opening HTTP upgrade request, up to and including its blank line.
    /// </summary>
    /// <remarks>
    /// Read byte by byte deliberately: reading in blocks would consume the first WebSocket frames along
    /// with the headers, and those must pass through untouched.
    /// </remarks>
    /// <param name="stream">The client stream.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The request text.</returns>
    private static async Task<string> ReadUpgradeRequestAsync(NetworkStream stream, CancellationToken ct)
    {
        var request = new StringBuilder();
        var buffer = new byte[1];

        while (!request.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
        {
            var read = await stream.ReadAsync(buffer, ct);
            if (read == 0)
                break;

            request.Append((char)buffer[0]);
        }

        return request.ToString();
    }

    /// <summary>
    /// Replaces the request's <c>Host</c> header with the venue's host.
    /// </summary>
    /// <param name="request">The request as the client sent it.</param>
    /// <param name="host">The host to address it to.</param>
    /// <returns>The rewritten request.</returns>
    private static string RewriteHost(string request, string host)
    {
        var lines = request.Split("\r\n");
        for (var i = 0; i < lines.Length; i++)
            if (lines[i].StartsWith("Host:", StringComparison.OrdinalIgnoreCase))
                lines[i] = $"Host: {host}";

        return string.Join("\r\n", lines);
    }

    /// <summary>
    /// Copies one stream into another until either ends.
    /// </summary>
    /// <param name="from">The stream to read.</param>
    /// <param name="to">The stream to write.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that completes when the copy ends.</returns>
    private static async Task CopyAsync(System.IO.Stream from, System.IO.Stream to, CancellationToken ct)
    {
        var buffer = new byte[16 * 1024];

        while (true)
        {
            var read = await from.ReadAsync(buffer, ct);
            if (read == 0)
                return;

            await to.WriteAsync(buffer.AsMemory(0, read), ct);
            await to.FlushAsync(ct);
        }
    }

    /// <summary>
    /// Socket handler implementation for the relay.
    /// </summary>
    private sealed class RelayHandler : IHandler
    {
        /// <summary>
        /// The relay to hand connections to.
        /// </summary>
        private readonly TestWebSocketRelay _relay;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayHandler"/> class.
        /// </summary>
        /// <param name="relay">The relay to hand connections to.</param>
        public RelayHandler(TestWebSocketRelay relay)
        {
            _relay = relay;
        }

        /// <summary>
        /// Handles an accepted connection.
        /// </summary>
        /// <param name="socket">The accepted socket.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task HandleAsync(Socket socket, CancellationToken ct) => _relay.HandleAsync(socket, ct);
    }
}
