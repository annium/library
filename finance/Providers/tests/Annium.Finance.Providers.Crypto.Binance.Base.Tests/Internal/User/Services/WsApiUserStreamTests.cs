using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Crypto.Binance.Base.Internal.User.Services;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Tests.Internal.User.Services;

/// <summary>
/// Pins how <see cref="WsApiUserStream"/> behaves on a request/response socket: what it sends, when it calls
/// itself connected, what it does with a refusal, and how it survives the two ways a subscription ends.
/// </summary>
/// <remarks>
/// Everything here is observable only from the server's side of the socket, and most of it only when the
/// server misbehaves on purpose. A live exchange will not refuse a subscription to order, will not drop a
/// connection to order, and will not send a malformed frame at all - so a live test of this component
/// asserts that the happy path works and nothing else.
/// </remarks>
public class WsApiUserStreamTests : ProvidersTestBase
{
    /// <summary>How long any test here may run before xUnit fails it, in milliseconds.</summary>
    private const int TimeoutMs = 60_000;

    /// <summary>How often the stream retries, for the tests that are about retrying.</summary>
    /// <remarks>
    /// Short, so a retry test does not wait on a clock, and not zero, so a refusal does not spin the local
    /// server while an assertion is being made.
    /// </remarks>
    private const int RetryMs = 50;

    /// <summary>How often the stream retries, for every test that is not about retrying.</summary>
    /// <remarks>
    /// Long enough that no retry can fire while a test is doing its own steps - a race the short interval
    /// above quietly created for every other test here. They read the subscribe request, make an assertion
    /// or two, then answer it; if a retry went out in between, the answer names an id that is no longer
    /// pending and is correctly ignored, so the test waits for a status that will never come and dies on
    /// its deadline.
    ///
    /// It passed locally and failed on a slower machine, which is the whole character of it: the margin
    /// was tens of milliseconds and nothing in the test said so.
    /// </remarks>
    private const int NoRetryMs = 30_000;

    /// <summary>
    /// Initializes a new instance of the <see cref="WsApiUserStreamTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public WsApiUserStreamTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// The first thing sent on a new connection is a signed subscription request.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Connected_SendsASignedSubscription()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());

        // act
        await using var stream = CreateStream(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        var frame = Parse(await connection.WaitMessageAsync(ct));

        // assert
        frame.GetProperty("method").GetString().Is("userDataStream.subscribe.signature");

        var parameters = frame.GetProperty("params");
        parameters.GetProperty("apiKey").GetString().Is("key");
        parameters.GetProperty("signature").GetString().Is("signature");
        parameters.GetProperty("timestamp").GetInt64().Is(1);
    }

    /// <summary>
    /// An open socket is not a subscribed account: the stream reports connected only once the venue has
    /// acknowledged the subscription.
    /// </summary>
    /// <remarks>
    /// The property this component exists for. Reported on the socket instead, a connector starts its
    /// loaders against a connection that is listening to nothing, and the account looks quiet rather than
    /// broken - which is the one failure a connection-based stream cannot tell you about.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task NotConnected_UntilTheSubscriptionIsAcknowledged()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        var connects = Channel.CreateUnbounded<int>();

        await using var stream = CreateStream(server, monitor);
        stream.OnConnected += () => connects.Writer.TryWrite(0);

        var connection = await server.WaitConnectionAsync(ct);
        var id = Parse(await connection.WaitMessageAsync(ct)).GetProperty("id").GetString().NotNull();

        // assert - the socket is up and the request is out, and neither of those is connected
        monitor.Status.IsNot(ConnectorStatus.Connected);
        connects.Reader.Count.Is(0);

        // act
        await connection.SendAsync(Acknowledgement(id), ct);

        // assert
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
        (await connects.Reader.ReadAsync(ct)).Is(0);
    }

    /// <summary>
    /// A reply carrying someone else's id does not count as the subscription being acknowledged.
    /// </summary>
    /// <remarks>
    /// Asserted by what the stream <b>does next</b> rather than by what it has not done yet. Sending a
    /// frame and then reading a status pins nothing: the send returns when the bytes leave, not when the
    /// other side has read them, so the status is read before the frame could have changed it and the
    /// assertion holds whatever the stream believes. This version waits for the retry that only an
    /// unacknowledged subscription produces - and a stream that took the wrong reply sends no retry, so it
    /// fails here instead of passing on a race.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task AReplyToAnotherRequest_IsNotTheSubscription()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());

        await using var stream = CreateStream(server, monitor, RetryMs);
        var connection = await server.WaitConnectionAsync(ct);
        var id = Parse(await connection.WaitMessageAsync(ct)).GetProperty("id").GetString().NotNull();

        // act
        await connection.SendAsync(Acknowledgement(id + "-not-this-one"), ct);

        // assert - still unsubscribed, so the retry goes out with an id of its own
        var retry = Parse(await connection.WaitMessageAsync(ct));
        retry.GetProperty("method").GetString().Is("userDataStream.subscribe.signature");
        var retryId = retry.GetProperty("id").GetString().NotNull();
        retryId.IsNot(id);
        monitor.Status.IsNot(ConnectorStatus.Connected);

        // and the reply to that one is taken, so this is not a stream that ignores every reply
        await connection.SendAsync(Acknowledgement(retryId), ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
    }

    /// <summary>
    /// A reconnected socket is subscribed again, on the new connection.
    /// </summary>
    /// <remarks>
    /// The silent failure this guards: a stream that reconnects and does not resubscribe holds an open
    /// socket that delivers nothing, forever, while reporting whatever it reported before.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Reconnected_SubscribesAgainOnTheNewConnection()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());

        await using var stream = CreateStream(server, monitor);
        var first = await server.WaitConnectionAsync(ct);
        var firstId = Parse(await first.WaitMessageAsync(ct)).GetProperty("id").GetString().NotNull();
        await first.SendAsync(Acknowledgement(firstId), ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        first.Drop();

        // assert
        var second = await server.WaitConnectionAsync(ct);
        var frame = Parse(await second.WaitMessageAsync(ct));
        frame.GetProperty("method").GetString().Is("userDataStream.subscribe.signature");

        // the second request is its own request, not the first one replayed - so the first one's answer,
        // if it is still in flight, cannot be mistaken for this connection's
        frame.GetProperty("id").GetString().IsNot(firstId);
    }

    /// <summary>
    /// A refused subscription is reported on the error channel and tried again.
    /// </summary>
    /// <remarks>
    /// Tried again rather than reconnected around: the socket is healthy and the subscription is the cheap
    /// half, so tearing the connection down would retry the expensive half of a failure that is not in it.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task ARefusedSubscription_IsReportedAndTriedAgain()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        var errors = Channel.CreateUnbounded<ConnectorError>();
        monitor.OnError += error => errors.Writer.TryWrite(error);

        await using var stream = CreateStream(server, monitor, RetryMs);
        var connection = await server.WaitConnectionAsync(ct);
        var id = Parse(await connection.WaitMessageAsync(ct)).GetProperty("id").GetString().NotNull();

        // act
        await connection.SendAsync(
            "{\"id\":\""
                + id
                + "\",\"status\":401,\"error\":{\"code\":-2015,\"msg\":\"Invalid API-key, IP, or permissions for action.\"}}",
            ct
        );

        // assert - the refusal reaches a consumer, carrying what the venue said
        var error = await errors.Reader.ReadAsync(ct);
        error.Message.Contains("401").IsTrue(error.Message);
        error.Message.Contains("-2015").IsTrue(error.Message);

        // and the stream tries again on the same connection, rather than giving up or reconnecting
        var retry = Parse(await connection.WaitMessageAsync(ct));
        retry.GetProperty("method").GetString().Is("userDataStream.subscribe.signature");
        server.AcceptedConnections.Is(1);
        monitor.Status.IsNot(ConnectorStatus.Connected);
    }

    /// <summary>
    /// An account event reaches the consumer as the event itself, with the transport's envelope removed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task AnEvent_ArrivesUnwrappedFromItsEnvelope()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        var messages = Channel.CreateUnbounded<string>();

        await using var stream = CreateStream(server, monitor);
        stream.OnMessage += data => messages.Writer.TryWrite(Encoding.UTF8.GetString(data.Span));

        var connection = await server.WaitConnectionAsync(ct);
        var id = Parse(await connection.WaitMessageAsync(ct)).GetProperty("id").GetString().NotNull();
        await connection.SendAsync(Acknowledgement(id), ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        await connection.SendAsync("""{"subscriptionId":0,"event":{"e":"someEvent","E":17,"x":"y"}}""", ct);

        // assert - the event object, and neither the envelope around it nor a field of the envelope
        var delivered = Parse(await messages.Reader.ReadAsync(ct));
        delivered.GetProperty("e").GetString().Is("someEvent");
        delivered.GetProperty("E").GetInt64().Is(17);
        delivered.TryGetProperty("subscriptionId", out _).IsFalse("the envelope reached the consumer");
        delivered.TryGetProperty("event", out _).IsFalse("the envelope reached the consumer");
    }

    /// <summary>
    /// A subscription the venue ends is started again, without the connection being torn down.
    /// </summary>
    /// <remarks>
    /// The second of the two ways a subscription stops, and the one with nothing to notice it by: the socket
    /// stays open and simply goes quiet. Left alone, the connector reports connected forever and the account
    /// stops changing.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task ATerminatedStream_IsSubscribedAgainOnTheSameConnection()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        var messages = Channel.CreateUnbounded<string>();

        await using var stream = CreateStream(server, monitor);
        stream.OnMessage += data => messages.Writer.TryWrite(Encoding.UTF8.GetString(data.Span));

        var connection = await server.WaitConnectionAsync(ct);
        var id = Parse(await connection.WaitMessageAsync(ct)).GetProperty("id").GetString().NotNull();
        await connection.SendAsync(Acknowledgement(id), ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        await connection.SendAsync("""{"subscriptionId":0,"event":{"e":"eventStreamTerminated","E":17}}""", ct);

        // assert
        var retry = Parse(await connection.WaitMessageAsync(ct));
        retry.GetProperty("method").GetString().Is("userDataStream.subscribe.signature");
        server.AcceptedConnections.Is(1);

        // and it is not handed on as an account event: nothing downstream knows what to do with it
        messages.Reader.Count.Is(0);
    }

    /// <summary>
    /// A frame that does not parse is dropped and the stream carries on.
    /// </summary>
    /// <remarks>
    /// Driven by a good frame after the bad one, so this asserts that the stream still works rather than
    /// that nothing observable happened.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task AnUnparseableFrame_IsDroppedAndTheStreamSurvives()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        var messages = Channel.CreateUnbounded<string>();

        await using var stream = CreateStream(server, monitor);
        stream.OnMessage += data => messages.Writer.TryWrite(Encoding.UTF8.GetString(data.Span));

        var connection = await server.WaitConnectionAsync(ct);
        var id = Parse(await connection.WaitMessageAsync(ct)).GetProperty("id").GetString().NotNull();
        await connection.SendAsync(Acknowledgement(id), ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        await connection.SendAsync("{ this is not json", ct);
        await connection.SendAsync("""{"subscriptionId":0,"event":{"e":"someEvent","E":17}}""", ct);

        // assert - the good frame arrives, so the bad one cost the connection nothing
        Parse(await messages.Reader.ReadAsync(ct)).GetProperty("e").GetString().Is("someEvent");
        monitor.Status.Is(ConnectorStatus.Connected);
        server.AcceptedConnections.Is(1);
    }

    /// <summary>
    /// Tearing the stream down closes the connection and stops it reporting.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Disposed_ClosesTheConnectionAndStopsCounting()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());

        // a second target, so the monitor still has one after the stream leaves - with none at all it
        // reads disconnected whatever the stream did, and the assertion below would hold for the wrong
        // reason
        var peer = monitor.CreateReporter();
        peer.Bind(new object(), ConnectorStatus.Connected);

        var stream = CreateStream(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        var id = Parse(await connection.WaitMessageAsync(ct)).GetProperty("id").GetString().NotNull();
        await connection.SendAsync(Acknowledgement(id), ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        await stream.DisposeAsync();

        // assert
        await connection.WhenClosed.WaitAsync(ct);

        // a disposed component is gone rather than disconnected: left bound, it sits in the monitor as a
        // disconnected target beside the live ones and nothing can read connected again
        monitor.Status.Is(ConnectorStatus.Connected);
    }

    /// <summary>
    /// The account's used weight, as the venue states it in a reply, reaches the limiter.
    /// </summary>
    /// <remarks>
    /// This transport has no headers, so the accounting the HTTP paths do from one had nowhere to come
    /// from and the subscription's weight was spent and counted nowhere. Harmless while the subscription
    /// is healthy and exactly wrong when it is not: a refused one retries on an interval, which is the
    /// moment an account can least afford traffic nothing is accounting for.
    ///
    /// The numbers below are the venue's own, recorded off the wire on 2026-09-21.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task TheVenuesStatedWeight_ReachesTheLimiter()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());

        await using var stream = CreateStream(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        var id = Parse(await connection.WaitMessageAsync(ct)).GetProperty("id").GetString().NotNull();

        // act
        await connection.SendAsync(
            "{\"id\":\""
                + id
                + "\",\"status\":200,\"result\":{\"subscriptionId\":0},\"rateLimits\":[{\"rateLimitType\":\"REQUEST_WEIGHT\","
                + "\"interval\":\"MINUTE\",\"intervalNum\":1,\"limit\":6000,\"count\":4}]}",
            ct
        );
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // assert
        _rateLimiter.ReportedWeights.Contains(4).IsTrue("the stated weight did not reach the limiter");
        _rateLimiter.ReportedLimits.Contains(6000).IsTrue("the stated ceiling did not reach the limiter");
    }

    /// <summary>
    /// A reply carrying no rate limits leaves the limiter as it was.
    /// </summary>
    /// <remarks>
    /// Absent and none are different, and only one of them means there is budget. Read as zero, a reply
    /// that simply did not state the weight would hand the account's whole allowance back.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task AReplyWithoutRateLimits_TellsTheLimiterNothing()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());

        await using var stream = CreateStream(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        var id = Parse(await connection.WaitMessageAsync(ct)).GetProperty("id").GetString().NotNull();

        // act
        await connection.SendAsync(Acknowledgement(id), ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // assert
        _rateLimiter.ReportedWeights.Count.Is(0, "a reply stating no weight was read as stating zero");
    }

    /// <summary>
    /// Builds a stream pointed at the given local server.
    /// </summary>
    /// <param name="server">The local server the stream connects to.</param>
    /// <param name="monitor">The monitor the stream reports into.</param>
    /// <param name="retryMs">How often it retries an unacknowledged subscription, in milliseconds.</param>
    /// <returns>The stream under test.</returns>
    private WsApiUserStream CreateStream(TestWebSocketServer server, StatusMonitor monitor, int retryMs = NoRetryMs) =>
        new(server.Uri, retryMs, new TestSignatureService(), _rateLimiter, monitor.CreateReporter(), Get<ILogger>());

    /// <summary>
    /// The limiter the stream reports the venue's stated weight into, so a test can read it back.
    /// </summary>
    private readonly RecordingRateLimiter _rateLimiter = new();

    /// <summary>
    /// A limiter that allows everything and remembers what it was told.
    /// </summary>
    private sealed class RecordingRateLimiter : IRateLimiter
    {
        /// <summary>Gets the weights reported, in order.</summary>
        public List<int> ReportedWeights { get; } = new();

        /// <summary>Gets the limits reported, in order.</summary>
        public List<int> ReportedLimits { get; } = new();

        /// <summary>Allows every request.</summary>
        /// <returns>Always true.</returns>
        public bool CanExecute() => true;

        /// <summary>Records the reported limit.</summary>
        /// <param name="limit">The limit reported.</param>
        public void UpdateLimit(int limit) => ReportedLimits.Add(limit);

        /// <summary>Records the reported weight.</summary>
        /// <param name="weight">The weight reported.</param>
        public void UsedWeight(int weight) => ReportedWeights.Add(weight);

        /// <summary>Ignores the block.</summary>
        /// <param name="duration">The duration to block for.</param>
        public void Block(TimeSpan duration) { }

        /// <summary>Does nothing; the limiter holds no resources here.</summary>
        public void Dispose() { }
    }

    /// <summary>
    /// The venue's answer to an accepted subscription.
    /// </summary>
    /// <param name="id">The id of the request being answered.</param>
    /// <returns>The reply frame.</returns>
    private static string Acknowledgement(string id) =>
        "{\"id\":\"" + id + "\",\"status\":200,\"result\":{\"subscriptionId\":0}}";

    /// <summary>
    /// Parses a frame.
    /// </summary>
    /// <param name="frame">The frame text.</param>
    /// <returns>Its root element.</returns>
    private static JsonElement Parse(string frame) => JsonDocument.Parse(frame).RootElement.Clone();

    /// <summary>
    /// A signature service with fixed answers, so a frame the test reads is fully determined.
    /// </summary>
    private sealed class TestSignatureService : ISignatureService
    {
        /// <summary>Gets a fixed server time.</summary>
        public long ServerTime => 1;

        /// <summary>Gets the api key.</summary>
        /// <returns>The api key.</returns>
        public string GetKey() => "key";

        /// <summary>Signs the given data.</summary>
        /// <param name="data">The data to sign.</param>
        /// <returns>The signature.</returns>
        public string GetSignature(string data) => "signature";
    }
}
