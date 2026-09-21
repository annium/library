using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Crypto.Binance.Base.Internal.User.Services;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Converters;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Contracts.Converters;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Net.Servers.Web;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Tests.Internal.User.Services;

/// <summary>
/// Pins what <see cref="ListenKeyResolver"/> tells its consumers on every transition it can make: the first
/// key, a confirmation, a changed key, a failure before the first key and after one, and a teardown that
/// lands while a request is in flight.
/// </summary>
/// <remarks>
/// The resolver is what keeps a user data stream alive, and every one of these transitions is a thing the
/// connector's consumers see - the key event opens the socket, the reset event closes it. None of it was
/// executed by any offline test, and a live test cannot produce a changed key or a failure on demand.
/// </remarks>
public class ListenKeyResolverTests : ProvidersTestBase
{
    /// <summary>
    /// How long any test here may run before xUnit fails it, in milliseconds.
    /// </summary>
    private const int TimeoutMs = 60_000;

    /// <summary>
    /// The listen key endpoint the resolver is pointed at.
    /// </summary>
    private const string Endpoint = "/listenKey";

    /// <summary>
    /// How many requests the scripted server has answered.
    /// </summary>
    private int _requests;

    /// <summary>The HTTP method of every request the server answered, in order.</summary>
    private readonly System.Collections.Concurrent.ConcurrentBag<string> _methods = [];

    /// <summary>The path of every request the server answered, in order.</summary>
    private readonly System.Collections.Concurrent.ConcurrentBag<string> _paths = [];

    /// <summary>Records what the resolver asked of the rate limiter.</summary>
    private readonly RecordingRateLimiter _limiter = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ListenKeyResolverTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public ListenKeyResolverTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the HTTP request factory with the converters a listen key response is read through.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.AddHttpRequestFactoryWithJsonSerializer(
            string.Empty,
            new JsonSerializerOptions()
                .ResetConverters()
                .AddConverter<ListenKeyResponseConverter>()
                .AddConverter<OperationResultConverter>()
        );
    }

    /// <summary>
    /// The first key fetched is raised and the resolver reports itself connected.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Fetch_RaisesTheKeyAndReportsConnected()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunListenKeyServer(_ => Key("abc"));
        var monitor = new StatusMonitor(Get<ILogger>());

        // act
        await using var resolver = CreateResolver(server, monitor, out var keys, out _);

        // assert
        var key = await keys.Reader.ReadAsync(ct);
        key.Is("abc");
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
    }

    /// <summary>
    /// A keep-alive answering with the same key raises nothing further.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Confirm_SameKeyIsQuiet()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunListenKeyServer(_ => Key("abc"));
        var monitor = new StatusMonitor(Get<ILogger>());
        await using var resolver = CreateResolver(server, monitor, out var keys, out var resets);
        (await keys.Reader.ReadAsync(ct)).Is("abc");

        // act
        await WaitRequestsAsync(3, ct);

        // assert
        keys.Reader.Count.Is(0);
        resets.Reader.Count.Is(0);
        monitor.Status.Is(ConnectorStatus.Connected);
    }

    /// <summary>
    /// A key that came back different is dropped, and the consumer is told to start over.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Change_ResetsTheKey()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunListenKeyServer(attempt => Key(attempt == 0 ? "abc" : "xyz"));
        var monitor = new StatusMonitor(Get<ILogger>());

        // the keep-alive interval is far longer than the fetch one, as it is on the exchange. The change
        // itself is only noticed at the next keep-alive, so the test pays that once; what it then measures
        // is the cadence the resolver returns to. With both intervals equal the bug is invisible
        const int confirmInterval = 3_000;
        await using var resolver = CreateResolver(server, monitor, out var keys, out var resets, confirmInterval);
        (await keys.Reader.ReadAsync(ct)).Is("abc");

        // act
        await resets.Reader.ReadAsync(ct);

        // assert
        // the key that replaced it is fetched anew rather than adopted in place, so the consumer reopens
        // its socket instead of keeping one bound to a key the exchange no longer knows - and it is asked
        // for at fetch cadence, since the reset has already closed the stream. Left in keep-alive mode the
        // resolver answers here a whole confirm interval later, which is the wait this window excludes
        var replacement = await keys
            .Reader.ReadAsync(ct)
            .AsTask()
            .WaitAsync(TimeSpan.FromMilliseconds(confirmInterval / 4), ct);
        replacement.Is("xyz");
    }

    /// <summary>
    /// A failure before any key has been fetched is reported and retried.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task FailureBeforeFirstKey_IsReportedAndRetried()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunListenKeyServer(_ => Refusal());
        var monitor = new StatusMonitor(Get<ILogger>());
        var errors = Channel.CreateUnbounded<ConnectorError>();

        // act
        await using var resolver = CreateResolver(server, monitor, out var keys, out _);
        monitor.OnError += error => errors.Writer.TryWrite(error);

        // assert
        await errors.Reader.ReadAsync(ct);
        await WaitRequestsAsync(2, ct);
        monitor.Status.Is(ConnectorStatus.Connecting);
        keys.Reader.Count.Is(0);
    }

    /// <summary>
    /// A failed keep-alive drops the key already held and tells the consumer to start over.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task FailureAfterFirstKey_ResetsTheKey()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunListenKeyServer(attempt => attempt == 0 ? Key("abc") : Refusal());
        var monitor = new StatusMonitor(Get<ILogger>());
        var errors = Channel.CreateUnbounded<ConnectorError>();
        await using var resolver = CreateResolver(server, monitor, out var keys, out var resets);
        (await keys.Reader.ReadAsync(ct)).Is("abc");
        monitor.OnError += error => errors.Writer.TryWrite(error);

        // act
        await resets.Reader.ReadAsync(ct);

        // assert
        await errors.Reader.ReadAsync(ct);
        monitor.Status.Is(ConnectorStatus.Connecting);
    }

    /// <summary>
    /// A teardown that lands while a request is in flight leaves the monitor rather than reporting into it.
    /// </summary>
    /// <remarks>
    /// The response arrives at a resolver that no longer exists. Acting on it would report through an
    /// unbound reporter, which throws - on the timer's thread, where nothing catches it.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task DisposeMidRequest_ReportsNothing()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var inFlight = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var server = RunListenKeyServer(async _ =>
        {
            inFlight.TrySetResult();
            // VSTHRD003: the gate is this test's own TaskCompletionSource, opened by the test thread
#pragma warning disable VSTHRD003
            await release.Task;
#pragma warning restore VSTHRD003

            return Response(HttpStatusCode.OK, """{"listenKey":"abc"}""");
        });

        var monitor = new StatusMonitor(Get<ILogger>());

        // a second target, so the monitor still has one after the resolver leaves
        var peer = monitor.CreateReporter();
        peer.Bind(new object(), ConnectorStatus.Connected);

        var resolver = CreateResolver(server, monitor, out var keys, out _);
        await inFlight.Task.WaitAsync(ct);

        // act
        var disposing = resolver.DisposeAsync();
        release.TrySetResult();
        await disposing;

        // assert
        await WaitRequestsAsync(1, ct);
        keys.Reader.Count.Is(0);
        monitor.Status.Is(ConnectorStatus.Connected);
    }

    /// <summary>
    /// Builds a resolver pointed at the given local server, with its two events drained into channels.
    /// </summary>
    /// <param name="server">The local server answering listen key requests.</param>
    /// <param name="monitor">The monitor the resolver reports into.</param>
    /// <param name="keys">The channel every fetched key is written into.</param>
    /// <param name="resets">The channel every reset is written into.</param>
    /// <param name="confirmInterval">The keep-alive interval, in milliseconds, once a key is held.</param>
    /// <returns>The resolver under test.</returns>
    private ListenKeyResolver CreateResolver(
        IServer server,
        StatusMonitor monitor,
        out Channel<string> keys,
        out Channel<int> resets,
        int confirmInterval = 50
    )
    {
        // short enough that a test does not wait on a clock, long enough that a retry loop does not
        // drown the local server in requests while an assertion is being made
        var listenKeyConfig = new ListenKeyConfiguration(50, confirmInterval);

        var fetched = Channel.CreateUnbounded<string>();
        var reset = Channel.CreateUnbounded<int>();
        keys = fetched;
        resets = reset;

        var resolver = new ListenKeyResolver(
            server.HttpUri(),
            listenKeyConfig,
            Endpoint,
            GetKeyed<IHttpRequestFactory>(string.Empty),
            new TestSignatureService(),
            _limiter,
            monitor.CreateReporter(),
            Get<ILogger>()
        );
        resolver.OnListenKeyFetched += key => fetched.Writer.TryWrite(key);
        resolver.OnListenKeyReset += () => reset.Writer.TryWrite(0);

        return resolver;
    }

    /// <summary>
    /// Every listen key request is a <c>POST</c> to the configured endpoint, and every one goes through
    /// the rate limiter.
    /// </summary>
    /// <remarks>
    /// Three facts that a verification census found defended by nothing, all of them on the connection
    /// path every user stream depends on. The method in particular has a documented alternative on some
    /// venues - a separate verb for the keep-alive - and the class comment here once claimed this module
    /// used it. It does not, and now that is asserted rather than believed.
    /// <para>
    /// The limiter matters beyond tidiness: this request is counted against the same budget as everything
    /// else, so a resolver that went around the limiter would spend budget the rest of the module thinks
    /// it still has.
    /// </para>
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task EveryRequest_IsAPostToTheEndpointThroughTheLimiter()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunListenKeyServer(_ => Key("a-key"));
        var monitor = new StatusMonitor(Get<ILogger>());

        // act - one fetch, then a keep-alive confirmation, so both cadences are covered
        await using var resolver = CreateResolver(server, monitor, out var keys, out _);
        await keys.Reader.ReadAsync(ct);
        await WaitRequestsAsync(2, ct);

        // assert
        var methods = _methods.Distinct().ToArray();
        methods.Length.Is(1, $"the resolver used more than one method: {string.Join(",", methods)}");
        methods[0].Is("POST", "the resolver used something other than POST");

        var paths = _paths.Distinct().ToArray();
        paths.Length.Is(1, $"the resolver called more than one path: {string.Join(",", paths)}");
        paths[0].Is(Endpoint, "the resolver called something other than the configured endpoint");
        (_limiter.Consulted >= 2).IsTrue(
            $"the limiter was consulted {_limiter.Consulted} times for {Volatile.Read(ref _requests)} requests, so some went around it"
        );
    }

    /// <summary>
    /// Starts a server answering listen key requests from a script, counting what it answered.
    /// </summary>
    /// <param name="respond">Builds the response for a request, given how many came before it.</param>
    /// <returns>The running server; dispose it to stop listening.</returns>
    private IServer RunListenKeyServer(Func<int, Task<(HttpStatusCode Code, string Body)>> respond) =>
        this.RunHttpServer(
            async (request, response) =>
            {
                _methods.Add(request.HttpMethod);
                _paths.Add(request.Url?.AbsolutePath ?? string.Empty);
                var attempt = Interlocked.Increment(ref _requests) - 1;
                var (code, body) = await respond(attempt);
                var payload = Encoding.UTF8.GetBytes(body);

                response.StatusCode(code);
                response.ContentType = MediaTypeNames.Application.Json;
                response.ContentLength64 = payload.Length;
                await response.OutputStream.WriteAsync(payload);
            }
        );

    /// <summary>
    /// Waits until the server has answered at least the given number of requests.
    /// </summary>
    /// <param name="count">The number of answered requests to wait for.</param>
    /// <param name="ct">The test's cancellation token.</param>
    /// <returns>A task that completes once that many requests have been answered.</returns>
    private async Task WaitRequestsAsync(int count, CancellationToken ct)
    {
        while (Volatile.Read(ref _requests) < count)
            await Task.Delay(10, ct);
    }

    /// <summary>
    /// A successful listen key response.
    /// </summary>
    /// <param name="value">The key the response carries.</param>
    /// <returns>The response to answer with.</returns>
    private static Task<(HttpStatusCode Code, string Body)> Key(string value) =>
        Task.FromResult(Response(HttpStatusCode.OK, $$"""{"listenKey":"{{value}}"}"""));

    /// <summary>
    /// A refusal, in the shape Binance sends one.
    /// </summary>
    /// <returns>The response to answer with.</returns>
    private static Task<(HttpStatusCode Code, string Body)> Refusal() =>
        Task.FromResult(Response(HttpStatusCode.BadRequest, """{"code":-1105,"msg":"refused"}"""));

    /// <summary>
    /// Pairs a status code with a body.
    /// </summary>
    /// <param name="code">The status code to answer with.</param>
    /// <param name="body">The body to answer with.</param>
    /// <returns>The response.</returns>
    private static (HttpStatusCode Code, string Body) Response(HttpStatusCode code, string body) => (code, body);

    /// <summary>
    /// A rate limiter that allows everything, so the test measures the resolver rather than the limiter.
    /// </summary>
    /// <summary>
    /// A rate limiter that allows everything and counts what it was asked, so a test can tell a request
    /// that went through the limiter from one that went around it.
    /// </summary>
    /// <remarks>
    /// The stub this replaced recorded nothing, which made the listen key's limiter accounting
    /// unfalsifiable: deleting the limiter from the request chain broke no test. On a venue that counts
    /// this request against the same budget as everything else, that is a fact worth keeping.
    /// </remarks>
    private sealed class RecordingRateLimiter : IRateLimiter
    {
        /// <summary>Gets how many times the limiter was consulted before a request went out.</summary>
        public int Consulted => _consulted;

        /// <summary>Backing counter for <see cref="Consulted"/>.</summary>
        private int _consulted;

        /// <summary>Allows every request, and records that it was asked.</summary>
        /// <returns>Always true.</returns>
        public bool CanExecute()
        {
            Interlocked.Increment(ref _consulted);

            return true;
        }

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
    /// A signature service that signs nothing, since no exchange checks what the local server is told.
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
