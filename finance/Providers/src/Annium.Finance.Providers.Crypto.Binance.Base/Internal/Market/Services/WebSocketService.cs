using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Crypto.Binance.Base.Market;
using Annium.Linq;
using Annium.Logging;
using Annium.Net.WebSockets;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Internal.Market.Services;

/// <summary>
/// Base class for market-data services that subscribe to Binance's public WebSocket stream, tracking the currently
/// subscribed topics and issuing <c>SUBSCRIBE</c>/<c>UNSUBSCRIBE</c> requests as needed.
/// </summary>
internal abstract class WebSocketService : IDisposable, ILogSubject
{
    /// <summary>Gets the logger used to trace connection and subscription activity.</summary>
    public ILogger Logger { get; }

    /// <summary>The underlying WebSocket client connected to Binance's market data stream.</summary>
    private readonly IClientWebSocket _socket;

    /// <summary>The set of topics currently subscribed to, re-sent on reconnect.</summary>
    private readonly HashSet<string> _topics = new();

    /// <summary>Guards <see cref="_topics"/>.</summary>
    /// <remarks>
    /// The set is written from two threads that have nothing in common: the caller's, on every subscribe
    /// and unsubscribe, and the socket's, on every reconnect. Unguarded that is a torn <see cref="HashSet{T}"/>,
    /// and the failure it produces - a lost or duplicated topic - is indistinguishable from an exchange
    /// that did not apply a subscription.
    /// </remarks>
    private readonly Lock _gate = new();

    /// <summary>The reporter used to publish connection status changes.</summary>
    private readonly IStatusReporter _statusReporter;

    /// <summary>Whether teardown has begun.</summary>
    /// <remarks>
    /// Read by the send path, which reports a failed control frame as an error - except during teardown,
    /// where the socket is gone by design and the reporter is unbound, so reporting would throw on a
    /// background task rather than tell anyone anything.
    /// </remarks>
    private volatile bool _isDisposed;

    /// <summary>Initializes a new instance of the <see cref="WebSocketService"/> class and starts connecting to the market WebSocket API.</summary>
    /// <param name="config">The market configuration providing the WebSocket API endpoint.</param>
    /// <param name="statusReporter">The reporter used to publish connection status changes.</param>
    /// <param name="logger">The logger to trace connection and subscription activity with.</param>
    protected WebSocketService(MarketConfigBase config, IStatusReporter statusReporter, ILogger logger)
    {
        Logger = logger;

        _socket = new ClientWebSocket(ClientWebSocketOptions.Default, logger);
        _socket.OnConnected += HandleConnected;
        _socket.OnDisconnected += HandleDisconnected;
        _socket.OnTextReceived += HandleData;
        // the user stream reports its socket's faults and this half never did, so a market connector that
        // kept failing looked no different from one merely reconnecting - and OnError is the only channel
        // a connector has for saying otherwise
        _socket.OnError += HandleError;

        _statusReporter = statusReporter;
        _statusReporter.Bind(this);

        // connecting is reported before the socket is asked to connect, and the order is the whole point.
        // Connect returns as soon as the attempt is under way, so with these two the other way round a
        // connection that completes in between is reported connected by the socket's own thread and then
        // overwritten here - permanently, because a socket already connected raises no second event. The
        // connector then reports itself connecting for as long as it lives while its socket works fine,
        // which is indistinguishable from a venue that is simply slow.
        //
        // Found as a test that hung once in four CI runs waiting for connected; the two user streams beside
        // this one always had the order right, which is why only the market side ever showed it
        _statusReporter.Connecting();
        _socket.Connect(new Uri(config.WsApi, config.WsUriPath));
    }

    /// <summary>Disconnects and disposes the underlying WebSocket, reporting the connector as disconnected.</summary>
    public void Dispose()
    {
        this.Trace("start");

        _isDisposed = true;

        // stop listening before unbinding, not after: a close or an error already in flight lands on
        // these handlers, and reporting against a reporter that has been unbound throws. Handlers left
        // attached across the unbind turned an ordinary teardown into an error in the log
        this.Trace("unhook socket handlers");
        _socket.OnConnected -= HandleConnected;
        _socket.OnDisconnected -= HandleDisconnected;
        _socket.OnTextReceived -= HandleData;
        _socket.OnError -= HandleError;

        this.Trace("signal disconnected");
        _statusReporter.Disconnected();

        // and stop counting: a disposed component is gone, not disconnected. Left registered, it sits
        // in the monitor as a disconnected target beside the live ones, and the connector can never
        // report itself connected again for as long as it lives
        _statusReporter.Unbind();

        this.Trace("dispose socket");
        _socket.Dispose();

        this.Trace("done");
    }

    /// <summary>Adds the given topics to the tracked subscription set and sends a <c>SUBSCRIBE</c> request for the ones not already subscribed to.</summary>
    /// <param name="topics">The topic names to subscribe to.</param>
    protected void SubscribeTopics(IEnumerable<string> topics)
    {
        this.Trace("start");

        List<string> targets;
        lock (_gate)
            targets = [.. topics.Where(topic => _topics.Add(topic))];

        if (targets.Count == 0)
        {
            this.Trace("skip - no topics to subscribe");
            return;
        }

        if (LogConfig.IsEnabled(LogLevel.Trace))
            this.Trace<string>("subscribe to {topics}", targets.Join(","));

        Send("SUBSCRIBE", targets);

        this.Trace("done");
    }

    /// <summary>Removes the given topics from the tracked subscription set and sends an <c>UNSUBSCRIBE</c> request for the ones that were subscribed to.</summary>
    /// <param name="topics">The topic names to unsubscribe from.</param>
    protected void UnsubscribeTopics(IEnumerable<string> topics)
    {
        this.Trace("start");

        List<string> targets;
        lock (_gate)
            targets = [.. topics.Where(topic => _topics.Remove(topic))];

        if (targets.Count == 0)
        {
            this.Trace("skip - no topics to unsubscribe");
            return;
        }

        if (LogConfig.IsEnabled(LogLevel.Trace))
            this.Trace<string>("unsubscribe from {topics}", targets.Join(","));

        Send("UNSUBSCRIBE", targets);

        this.Trace("done");
    }

    /// <summary>Sends a control request and reports it on the error channel if it did not go out.</summary>
    /// <remarks>
    /// The send is not awaited by the caller - subscribing is a synchronous call on a hot path - but its
    /// result is observed. Dropped, a subscribe that never reached the exchange left a connector that
    /// looked subscribed and delivered nothing, which is indistinguishable from a quiet symbol.
    /// </remarks>
    /// <param name="method">The control method being sent, named in the error if it fails.</param>
    /// <param name="topics">The topic names the request applies to.</param>
    private void Send(string method, IReadOnlyCollection<string> topics)
    {
        var request = new Request { Method = method, Params = topics };
        var payload = JsonSerializer.SerializeToUtf8Bytes(request);

        // not awaited: subscribing is a synchronous call, and on reconnect this runs on the socket's own
        // thread, which must not be blocked waiting for a frame to go out. The result is observed inside
        _ = SendAsync(method, payload);
    }

    /// <summary>Awaits a control request's send and reports a failure on the error channel.</summary>
    /// <param name="method">The control method being sent, named in the error if it fails.</param>
    /// <param name="payload">The serialized request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SendAsync(string method, ReadOnlyMemory<byte> payload)
    {
        try
        {
            var status = await _socket.SendTextAsync(payload);
            if (status is WebSocketSendStatus.Ok)
                return;

            // a teardown makes every in-flight send fail, by design, and the reporter is already unbound
            if (_isDisposed)
            {
                this.Trace("skip {method} send failure: {status} - disposed", method, status);
                return;
            }

            this.Trace("{method} send failed: {status}", method, status);
            _statusReporter.Error(new ConnectorError($"failed to send {method}: {status}"));
        }
        catch (Exception ex)
        {
            // nothing awaits this task, so an exception escaping it is lost entirely - including the
            // InvalidOperationException a report against an unbound reporter throws if teardown wins a race
            this.Error(ex);
        }
    }

    /// <summary>Handles a raw text message received over the WebSocket, deserializing and dispatching it to derived-class subscribers.</summary>
    /// <param name="raw">The raw UTF-8 text payload received over the WebSocket.</param>
    protected abstract void HandleData(ReadOnlyMemory<byte> raw);

    /// <summary>Re-sends a <c>SUBSCRIBE</c> request for all currently tracked topics, then reports the connector as connected.</summary>
    private void HandleConnected()
    {
        this.Trace("start");

        string[] targets;
        lock (_gate)
            targets = _topics.ToArray();

        if (targets.Length > 0)
        {
            if (LogConfig.IsEnabled(LogLevel.Trace))
                this.Trace<string>("subscribe to {topics}", targets.Join(","));

            Send("SUBSCRIBE", targets);
        }
        else
            this.Trace("skip - no topics to subscribe");

        // reported last, so that connected means connected and resubscribed. Reported first, it named a
        // moment at which the tracked set had not been re-sent yet, and a subscribe issued on that signal
        // raced this one - both sides sending the same topic, which is how the same subscribe went out twice
        this.Trace("signal connected");
        _statusReporter.Connected();

        this.Trace("done");
    }

    /// <summary>Reports the connector as reconnecting after the WebSocket closes.</summary>
    /// <param name="status">The close status reported by the WebSocket.</param>
    private void HandleDisconnected(WebSocketCloseStatus status)
    {
        this.Trace("start");

        // no error reported here even when the status carries one: the socket raises OnError alongside
        // every error-carrying close, always with the exception attached, so HandleError has already said
        // it - and better, with the reason rather than a fixed string
        this.Trace("signal disconnected: {status}", status);
        _statusReporter.Connecting();

        this.Trace("done");
    }

    /// <summary>Reports a WebSocket error and switches the connector back to reconnecting.</summary>
    /// <param name="error">The exception raised by the WebSocket.</param>
    private void HandleError(Exception error)
    {
        this.Trace("start");

        _statusReporter.Error(new ConnectorError(error.ToString()));
        _statusReporter.Connecting();

        this.Trace("done");
    }

    /// <summary>The JSON payload of a Binance WebSocket <c>SUBSCRIBE</c>/<c>UNSUBSCRIBE</c> control request.</summary>
    private record Request
    {
        /// <summary>The last request id issued, used to hand out unique, monotonically increasing ids.</summary>
        private static long _lastId;

        /// <summary>Gets a new unique id for this request, assigned on every read.</summary>
        [JsonPropertyName("id")]
        public long Id => Interlocked.Increment(ref _lastId);

        /// <summary>Gets the control method, either <c>SUBSCRIBE</c> or <c>UNSUBSCRIBE</c>.</summary>
        [JsonPropertyName("method")]
        public required string Method { get; init; }

        /// <summary>Gets the topic names the request applies to.</summary>
        [JsonPropertyName("params")]
        public required IReadOnlyCollection<string> Params { get; init; }
    }
}
