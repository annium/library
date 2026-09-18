using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Annium.Net.WebSockets;
using Annium.Testing;

namespace Annium.Finance.Providers.Tests.Lib.Infrastructure;

/// <summary>
/// Runs a local WebSocket server for a <see cref="TestBase"/>-derived test, so a provider's stream services
/// can be driven against a socket the test controls instead of the exchange's.
/// </summary>
/// <remarks>
/// The counterpart of <see cref="TestBaseHttpServerExtensions"/> for the other half of a connector. A stream
/// service's whole contract - what it sends on subscribe, what it re-sends on reconnect, what it does with a
/// payload that does not parse, what it reports when the connection drops - is observable only from the
/// server side of its socket, and until this existed there was no server side to observe it from.
/// </remarks>
public static class TestBaseWebSocketServerExtensions
{
    /// <summary>
    /// Starts a local WebSocket server that accepts every connection and hands each one to the test.
    /// </summary>
    /// <param name="test">The test instance the server resolves its services from.</param>
    /// <returns>The running server; dispose it to stop listening.</returns>
    public static TestWebSocketServer RunWebSocketServer(this TestBase test)
    {
        var sp = test.Get<IServiceProvider>();

        return new TestWebSocketServer(sp, sp.Resolve<ILogger>());
    }
}

/// <summary>
/// A local WebSocket server that queues every connection it accepts for the test to pick up.
/// </summary>
/// <remarks>
/// Connections are queued rather than handed to a callback, because the properties worth pinning are about
/// the sequence of connections: a client that reconnects produces a second one, and the test's assertion is
/// about what arrives on it. Awaiting the next connection keeps that sequence explicit.
/// </remarks>
public sealed class TestWebSocketServer : IAsyncDisposable, ILogSubject
{
    /// <summary>
    /// Gets the logger this server and its connections trace through.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the URI a client connects to.
    /// </summary>
    public Uri Uri => _server.WebSocketsUri();

    /// <summary>
    /// Gets the number of connections accepted so far, including ones already closed.
    /// </summary>
    public int AcceptedConnections => Volatile.Read(ref _acceptedConnections);

    /// <summary>
    /// The underlying web server.
    /// </summary>
    private readonly IServer _server;

    /// <summary>
    /// Connections accepted and not yet taken by the test.
    /// </summary>
    private readonly Channel<TestWebSocketConnection> _connections = Channel.CreateUnbounded<TestWebSocketConnection>();

    /// <summary>
    /// The number of connections accepted so far.
    /// </summary>
    private int _acceptedConnections;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestWebSocketServer"/> class and starts listening.
    /// </summary>
    /// <param name="sp">The service provider the server is built from.</param>
    /// <param name="logger">The logger to trace through.</param>
    internal TestWebSocketServer(IServiceProvider sp, ILogger logger)
    {
        Logger = logger;
        _server = ServerBuilder.New(sp).WithWebSocketHandler(new WebSocketHandler(HandleAsync)).Start().NotNull();
        this.Trace("started websocket server at port {port}", _server.Port);
    }

    /// <summary>
    /// Takes the next accepted connection, waiting for one if none is queued.
    /// </summary>
    /// <param name="ct">The test's cancellation token, so a connection that never lands ends with the test.</param>
    /// <returns>The next connection the server accepted.</returns>
    public async Task<TestWebSocketConnection> WaitConnectionAsync(CancellationToken ct) =>
        await _connections.Reader.ReadAsync(ct);

    /// <summary>
    /// Stops the server.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        await _server.DisposeAsync();
        _connections.Writer.TryComplete();

        this.Trace("done");
    }

    /// <summary>
    /// Wraps an accepted socket, queues it for the test, and keeps the handler alive until it closes.
    /// </summary>
    /// <param name="ctx">The accepted WebSocket context.</param>
    /// <param name="ct">The server's cancellation token.</param>
    /// <returns>A task that completes when the connection closes.</returns>
    private async Task HandleAsync(HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        this.Trace("start");

        var socket = new ServerWebSocket(ctx.WebSocket, Logger, ct);
        var connection = new TestWebSocketConnection(socket, ctx.RequestUri, Logger);

        // only now: the connection has attached its handler, so the frame a reconnecting client sends
        // immediately cannot be read before there is anyone to read it to
        socket.Start();

        Interlocked.Increment(ref _acceptedConnections);
        _connections.Writer.TryWrite(connection);

        try
        {
            // the handler owns the socket's lifetime: returning from it lets the server tear the context
            // down, so a connection the test still holds has to keep this frame alive until it closes
            await socket.WhenDisconnectedAsync(ct);
        }
        catch (OperationCanceledException)
        {
            this.Trace("cancelled");
        }
        finally
        {
            connection.Close();
        }

        this.Trace("done");
    }
}

/// <summary>
/// One accepted connection, from the server's side of it.
/// </summary>
public sealed class TestWebSocketConnection : ILogSubject
{
    /// <summary>
    /// Gets the logger this connection traces through.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets a task that completes when the connection closes, whichever side closed it.
    /// </summary>
    public Task WhenClosed => _closed.Task;

    /// <summary>
    /// Gets the URI the client connected to.
    /// </summary>
    /// <remarks>
    /// What a stream service puts in the URL is part of its contract - a Binance user stream carries its
    /// listen key there, and nothing else the test can observe says which key it connected with.
    /// </remarks>
    public Uri RequestUri { get; }

    /// <summary>
    /// The server side of the socket.
    /// </summary>
    private readonly IServerWebSocket _socket;

    /// <summary>
    /// Text frames received and not yet taken by the test.
    /// </summary>
    private readonly Channel<string> _messages = Channel.CreateUnbounded<string>();

    /// <summary>
    /// Every text frame received on this connection, in order.
    /// </summary>
    private readonly List<string> _received = new();

    /// <summary>
    /// Guards <see cref="_received"/>, written from the socket's receive loop and read by the test.
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// Signals <see cref="WhenClosed"/>.
    /// </summary>
    private readonly TaskCompletionSource _closed = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Initializes a new instance of the <see cref="TestWebSocketConnection"/> class.
    /// </summary>
    /// <param name="socket">The server side of the accepted socket.</param>
    /// <param name="requestUri">The URI the client connected to.</param>
    /// <param name="logger">The logger to trace through.</param>
    internal TestWebSocketConnection(IServerWebSocket socket, Uri requestUri, ILogger logger)
    {
        Logger = logger;
        RequestUri = requestUri;
        _socket = socket;
        _socket.OnTextReceived += HandleTextReceived;
    }

    /// <summary>
    /// Gets everything received on this connection so far, in order.
    /// </summary>
    /// <returns>A snapshot of the text frames received.</returns>
    public IReadOnlyList<string> Received
    {
        get
        {
            lock (_locker)
                return _received.ToArray();
        }
    }

    /// <summary>
    /// Takes the next text frame, waiting for one if none has arrived yet.
    /// </summary>
    /// <param name="ct">The test's cancellation token, so a frame that never arrives ends with the test.</param>
    /// <returns>The next text frame received.</returns>
    public async Task<string> WaitMessageAsync(CancellationToken ct) => await _messages.Reader.ReadAsync(ct);

    /// <summary>
    /// Sends a text frame to the client.
    /// </summary>
    /// <param name="text">The text to send.</param>
    /// <param name="ct">The test's cancellation token.</param>
    /// <returns>The send status the socket reported.</returns>
    public async Task<WebSocketSendStatus> SendAsync(string text, CancellationToken ct = default)
    {
        this.Trace("send {length} chars", text.Length);

        return await _socket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);
    }

    /// <summary>
    /// Closes the connection from the server's side, as an exchange dropping a client does.
    /// </summary>
    /// <remarks>
    /// The point of the helper: the client's reconnect and resubscribe behaviour is unreachable without a
    /// way to end a connection the client did not end.
    /// </remarks>
    public void Drop()
    {
        this.Trace("start");

        _socket.Disconnect();

        this.Trace("done");
    }

    /// <summary>
    /// Marks the connection closed once the server handler has finished with it.
    /// </summary>
    internal void Close()
    {
        this.Trace("start");

        _socket.OnTextReceived -= HandleTextReceived;
        _messages.Writer.TryComplete();
        _closed.TrySetResult();
        _socket.Dispose();

        this.Trace("done");
    }

    /// <summary>
    /// Records a received text frame and queues it for the test.
    /// </summary>
    /// <param name="data">The raw UTF-8 payload received.</param>
    private void HandleTextReceived(ReadOnlyMemory<byte> data)
    {
        var text = Encoding.UTF8.GetString(data.Span);

        lock (_locker)
            _received.Add(text);

        _messages.Writer.TryWrite(text);
    }
}

/// <summary>
/// WebSocket handler implementation for test servers.
/// </summary>
file class WebSocketHandler : IWebSocketHandler
{
    /// <summary>
    /// The function to handle accepted connections.
    /// </summary>
    private readonly Func<HttpListenerWebSocketContext, CancellationToken, Task> _handle;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketHandler"/> class.
    /// </summary>
    /// <param name="handle">The function to handle accepted connections.</param>
    public WebSocketHandler(Func<HttpListenerWebSocketContext, CancellationToken, Task> handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Handles an accepted WebSocket connection.
    /// </summary>
    /// <param name="ctx">The accepted WebSocket context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task HandleAsync(HttpListenerWebSocketContext ctx, CancellationToken ct) => _handle(ctx, ct);
}
