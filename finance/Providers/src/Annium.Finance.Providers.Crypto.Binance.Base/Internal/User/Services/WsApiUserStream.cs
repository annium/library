using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Logging;
using Annium.Net.WebSockets;
using Annium.Threading;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Internal.User.Services;

/// <summary>
/// Delivers a Binance account's events over the WebSocket API, where the subscription is a signed method call
/// on the same connection the events then arrive on.
/// </summary>
/// <remarks>
/// <para>
/// The other implementation of this interface fetches a key over REST, spends it in a URL and keeps it alive
/// on a timer. This one does none of that: the connection is opened to a fixed endpoint and the account is
/// named by a signature sent over it. The venue this was written for deprecated the key mechanism and
/// removed it from its reference entirely - see §11 of the provider manifest.
/// </para>
/// <para>
/// Two frames arrive on one socket and they are told apart by shape: a reply carries an <c>id</c>, an event
/// carries an <c>event</c>. The envelope is unwrapped here rather than by the converters downstream, so that
/// both implementations of this interface hand a consumer the same thing - the event object itself. A
/// converter that had to know which transport delivered it would be a second place the transport is
/// recorded.
/// </para>
/// </remarks>
internal class WsApiUserStream : IUserStream, ILogSubject
{
    /// <summary>The method that subscribes the account's events onto this connection.</summary>
    /// <remarks>
    /// The signature variant, not the session one. The session route requires a key of a type the account
    /// may not have, and making the stream depend on that turns a code change into a change the account
    /// holder has to make; this one is signed per request with the key every other path already uses.
    /// </remarks>
    private const string SubscribeMethod = "userDataStream.subscribe.signature";

    /// <summary>The event the venue sends when a subscription has ended and the socket has not.</summary>
    private const string StreamTerminatedEvent = "eventStreamTerminated";

    /// <summary>The rate-limit row naming the request-weight budget.</summary>
    private const string RequestWeightLimit = "REQUEST_WEIGHT";

    /// <summary>The interval of the request-weight budget the limiter models.</summary>
    private const string MinuteInterval = "MINUTE";

    /// <summary>Gets the logger used to trace connection and subscription activity.</summary>
    public ILogger Logger { get; }

    /// <summary>Raised once the account's events are subscribed onto the connection.</summary>
    public event Action OnConnected = delegate { };

    /// <summary>Raised when the connection drops.</summary>
    public event Action OnDisconnected = delegate { };

    /// <summary>Raised for every account event received, already unwrapped from its envelope.</summary>
    public event Action<ReadOnlyMemory<byte>> OnMessage = delegate { };

    /// <summary>The endpoint of the WebSocket API.</summary>
    private readonly Uri _wsApi;

    /// <summary>How often a refused subscription is attempted again.</summary>
    private readonly int _subscribeRetryInterval;

    /// <summary>Signs the subscription request.</summary>
    private readonly ISignatureService _signatureService;

    /// <summary>The limiter this stream's requests are counted against.</summary>
    /// <remarks>
    /// The subscription is cheap and easy to leave uncounted, and leaving it uncounted is exactly wrong
    /// when it is not healthy: a refused subscription retries on an interval, which is the moment the
    /// account can least afford traffic nothing is accounting for. The venue states the account's used
    /// weight in the reply, which is the same thing the HTTP paths read from a header.
    /// </remarks>
    private readonly IRateLimiter _rateLimiter;

    /// <summary>The reporter used to publish connection status changes.</summary>
    private readonly IStatusReporter _statusReporter;

    /// <summary>The underlying socket.</summary>
    private readonly IClientWebSocket _socket;

    /// <summary>Drives subscription attempts, armed while the socket is up and unsubscribed.</summary>
    private readonly ISequentialTimer _timer;

    /// <summary>The disposable box tearing this stream down.</summary>
    private readonly AsyncDisposableBox _disposable;

    /// <summary>Guards <see cref="_pendingSubscribeId"/>.</summary>
    private readonly Lock _gate = new();

    /// <summary>The id of the subscription request awaiting a reply, or empty when none is.</summary>
    /// <remarks>
    /// Matched against, rather than ignored, because a reply is not necessarily to the request just sent:
    /// a refusal retried across a reconnect leaves an earlier attempt's answer in flight, and accepting it
    /// would report a subscription that belongs to a connection that no longer exists.
    /// </remarks>
    private string _pendingSubscribeId = string.Empty;

    /// <summary>The last request id issued.</summary>
    private long _lastId;

    /// <summary>Whether teardown has begun.</summary>
    private volatile bool _isDisposed;

    /// <summary>Initializes a new instance of the <see cref="WsApiUserStream"/> class and starts connecting.</summary>
    /// <param name="wsApi">The endpoint of the WebSocket API.</param>
    /// <param name="subscribeRetryInterval">How often a refused subscription is attempted again, in milliseconds.</param>
    /// <param name="signatureService">Signs the subscription request.</param>
    /// <param name="rateLimiter">The limiter this stream's requests are counted against.</param>
    /// <param name="statusReporter">The reporter used to publish connection status changes.</param>
    /// <param name="logger">The logger to trace through.</param>
    public WsApiUserStream(
        Uri wsApi,
        int subscribeRetryInterval,
        ISignatureService signatureService,
        IRateLimiter rateLimiter,
        IStatusReporter statusReporter,
        ILogger logger
    )
    {
        Logger = logger;
        _wsApi = wsApi;
        _subscribeRetryInterval = subscribeRetryInterval;
        _signatureService = signatureService;
        _rateLimiter = rateLimiter;

        _statusReporter = statusReporter;
        _statusReporter.Bind(this);
        _statusReporter.Connecting();

        _disposable = Disposable.AsyncBox(logger);

        _socket = new ClientWebSocket(ClientWebSocketOptions.Default, logger);
        _socket.OnConnected += HandleConnected;
        _socket.OnDisconnected += HandleDisconnected;
        _socket.OnTextReceived += HandleData;
        _socket.OnError += HandleError;

        // created disarmed: there is nothing to subscribe until the socket is up, and a timer that starts
        // running before that sends its first attempt into a connection that does not exist
        _timer = Timers.Async(SubscribeAsync, Timeout.Infinite, _subscribeRetryInterval, logger);
        _disposable += (IAsyncDisposable)_timer;

        _socket.Connect(_wsApi);
    }

    /// <summary>Tears the stream down: stops listening, stops retrying, and reports itself gone.</summary>
    /// <returns>A value task representing the asynchronous teardown.</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        _isDisposed = true;

        // stop listening before unbinding, not after: a close or an error already in flight lands on these
        // handlers, and reporting against a reporter that has been unbound throws
        this.Trace("unhook socket handlers");
        _socket.OnConnected -= HandleConnected;
        _socket.OnDisconnected -= HandleDisconnected;
        _socket.OnTextReceived -= HandleData;
        _socket.OnError -= HandleError;

        await _disposable.DisposeAsync();

        this.Trace("signal disconnected");
        _statusReporter.Disconnected();

        // and stop counting: a disposed component is gone, not disconnected. Left registered, it sits in
        // the monitor as a disconnected target beside the live ones, and the connector can never report
        // itself connected again for as long as it lives
        _statusReporter.Unbind();

        this.Trace("dispose socket");
        _socket.Dispose();

        this.Trace("done");
    }

    /// <summary>Arms the subscription timer once the socket is up.</summary>
    /// <remarks>
    /// Connected is deliberately <b>not</b> reported here. The socket being open says nothing about the
    /// account's events reaching it - on this protocol they do not until a method call says so - and a
    /// connector told it is connected starts its loaders against a stream that is listening to nothing.
    /// </remarks>
    private void HandleConnected()
    {
        this.Trace("start");

        _timer.Change(0, _subscribeRetryInterval);

        this.Trace("done");
    }

    /// <summary>Disarms the timer, forgets any pending subscription, and reports the drop.</summary>
    /// <param name="status">The close status reported by the socket.</param>
    private void HandleDisconnected(WebSocketCloseStatus status)
    {
        this.Trace("start: {status}", status);

        _timer.Change(Timeout.Infinite, _subscribeRetryInterval);

        lock (_gate)
            _pendingSubscribeId = string.Empty;

        // no error reported here even when the status carries one: the socket raises OnError alongside
        // every error-carrying close, always with the exception attached, so HandleError has already said
        // it - and better, with the reason rather than a fixed string
        _statusReporter.Connecting();
        OnDisconnected();

        this.Trace("done");
    }

    /// <summary>Reports a socket error and switches back to reconnecting.</summary>
    /// <param name="error">The exception raised by the socket.</param>
    private void HandleError(Exception error)
    {
        this.Trace("start");

        _statusReporter.Error(new ConnectorError(error.ToString()));
        _statusReporter.Connecting();

        this.Trace("done");
    }

    /// <summary>Sends a signed subscription request and reports it if it did not go out.</summary>
    /// <returns>A value task representing the asynchronous send.</returns>
    private async ValueTask SubscribeAsync()
    {
        if (_isDisposed)
            return;

        var id = Interlocked.Increment(ref _lastId).ToString(CultureInfo.InvariantCulture);

        // recorded before the send, not after: the reply can arrive while the send's continuation is still
        // queued, and a reply matched against an id not yet written is a subscription silently discarded
        lock (_gate)
            _pendingSubscribeId = id;

        var payload = WsApiRequestBuilder.BuildSigned(id, SubscribeMethod, _signatureService);

        try
        {
            var status = await _socket.SendTextAsync(payload);
            if (status is WebSocketSendStatus.Ok)
            {
                this.Trace<string>("subscribe {id} sent", id);
                return;
            }

            // a teardown makes every in-flight send fail, by design, and the reporter is already unbound
            if (_isDisposed)
            {
                this.Trace("skip subscribe send failure: {status} - disposed", status);
                return;
            }

            this.Trace("subscribe send failed: {status}", status);
            _statusReporter.Error(new ConnectorError($"failed to send {SubscribeMethod}: {status}"));
        }
        catch (Exception ex)
        {
            // nothing awaits this task, so an exception escaping it is lost entirely - including the
            // InvalidOperationException a report against an unbound reporter throws if teardown wins a race
            this.Error(ex);
        }
    }

    /// <summary>
    /// Routes an incoming frame: a reply to the subscription, an account event, or something neither.
    /// </summary>
    /// <param name="raw">The raw UTF-8 text payload received.</param>
    private void HandleData(ReadOnlyMemory<byte> raw)
    {
        // guarded on the level, not only written at it: decoding the frame is the whole payload, and an
        // argument is evaluated before anything looks at whether the line would be discarded. The HTTP
        // side logs its bodies; without this the half of the account that arrives over a socket was the
        // half no log ever recorded
        if (LogConfig.IsEnabled(LogLevel.Trace))
            this.Trace<string>("frame: {frame}", Encoding.UTF8.GetString(raw.Span));

        try
        {
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;

            if (root.ValueKind is not JsonValueKind.Object)
            {
                this.Trace("drop frame: not an object");
                return;
            }

            if (root.TryGetProperty("id", out var id))
            {
                HandleReply(id, root);
                return;
            }

            if (root.TryGetProperty("event", out var payload))
            {
                HandleEvent(payload);
                return;
            }

            this.Trace("drop frame: neither a reply nor an event");
        }
        catch (JsonException ex)
        {
            // a frame that does not parse is dropped and the stream carries on. Tearing the connection down
            // over one would turn a single malformed payload into an outage, and the account's other events
            // are unaffected by it
            this.Trace<string>("drop unparseable frame: {error}", ex.Message);
        }
    }

    /// <summary>Handles a reply frame, reporting the subscription as live or as refused.</summary>
    /// <param name="id">The reply's id element.</param>
    /// <param name="root">The reply frame.</param>
    private void HandleReply(JsonElement id, JsonElement root)
    {
        var status = root.TryGetProperty("status", out var statusElement) ? statusElement.GetInt32() : 0;

        // read from every reply, refusals included: a refusal costs weight too, and the retry that
        // follows it is the traffic most worth accounting for
        ReportUsedWeight(root);

        if (status is not 200)
        {
            // reported whether or not it matches a pending request: a refusal carries why, and the commonest
            // reasons - a key that is not permitted, a clock too far out - are true of every attempt, so
            // dropping the ones that arrive late would hide the answer that explains all of them
            var error = root.TryGetProperty("error", out var errorElement) ? errorElement.GetRawText() : "unknown";
            this.Trace<int, string>("subscribe refused: {status} {error}", status, error);
            _statusReporter.Error(new ConnectorError($"{SubscribeMethod} refused with {status}: {error}"));

            // the timer is left armed, so this is retried rather than abandoned. The socket is healthy and
            // the subscription is cheap; tearing the connection down would retry the expensive half of a
            // failure whose cause is in the other half
            return;
        }

        if (id.ValueKind is not JsonValueKind.String || id.GetString() is not { } replyId)
        {
            this.Trace("drop reply: no id to match");
            return;
        }

        lock (_gate)
        {
            if (_pendingSubscribeId != replyId)
            {
                this.Trace<string>("drop reply {id}: not the pending subscription", replyId);
                return;
            }

            _pendingSubscribeId = string.Empty;
        }

        this.Trace<string>("subscribed, reply {id}", replyId);

        // stop retrying, then say so. Reported in this order so that connected means subscribed and settled,
        // rather than naming a moment at which another attempt was still due to go out
        _timer.Change(Timeout.Infinite, _subscribeRetryInterval);

        OnConnected();
        _statusReporter.Connected();
    }

    /// <summary>
    /// Reports the account's used weight, as the venue states it in a reply.
    /// </summary>
    /// <remarks>
    /// The same accounting the HTTP paths do from a response header, for a transport that has no headers.
    /// Only the one-minute request-weight row is read, because that is the budget the limiter models; a
    /// reply that carries none leaves the limiter as it was rather than being read as zero, since absent
    /// and none are different and only one of them means there is budget.
    /// </remarks>
    /// <param name="root">The reply frame.</param>
    private void ReportUsedWeight(JsonElement root)
    {
        if (!root.TryGetProperty("rateLimits", out var limits) || limits.ValueKind is not JsonValueKind.Array)
            return;

        foreach (var limit in limits.EnumerateArray())
        {
            if (
                !limit.TryGetProperty("rateLimitType", out var type)
                || type.GetString() != RequestWeightLimit
                || !limit.TryGetProperty("interval", out var interval)
                || interval.GetString() != MinuteInterval
                || !limit.TryGetProperty("intervalNum", out var intervalNum)
                || intervalNum.GetInt32() != 1
            )
                continue;

            if (limit.TryGetProperty("limit", out var ceiling))
                _rateLimiter.UpdateLimit(ceiling.GetInt32());

            if (limit.TryGetProperty("count", out var used))
            {
                this.Trace<string>("used weight reported as {used}", used.GetInt32().ToString());
                _rateLimiter.UsedWeight(used.GetInt32());
            }

            return;
        }
    }

    /// <summary>Unwraps an account event and hands it on, or re-subscribes if it says the stream has ended.</summary>
    /// <param name="payload">The <c>event</c> member of the frame.</param>
    private void HandleEvent(JsonElement payload)
    {
        if (payload.ValueKind is not JsonValueKind.Object)
        {
            this.Trace("drop event: payload is not an object");
            return;
        }

        if (
            payload.TryGetProperty("e", out var type)
            && type.ValueKind is JsonValueKind.String
            && type.GetString() == StreamTerminatedEvent
        )
        {
            // the subscription ended while the socket did not, so there is nothing to reconnect - only
            // something to subscribe again. Without this the socket stays open and silent, which is the one
            // failure a connection-based stream has no way to notice
            this.Trace("stream terminated, re-subscribing");
            _statusReporter.Connecting();
            _timer.Change(0, _subscribeRetryInterval);
            return;
        }

        OnMessage(Encoding.UTF8.GetBytes(payload.GetRawText()));
    }
}
