using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Social.Telegram.Internal.Integration.Receivers;
using Annium.Testing;
using Xunit;

namespace Annium.Social.Telegram.Tests;

/// <summary>
/// Tests for the webhook receiver: both halves are driven for real — the outbound setWebhook call goes to a
/// local stand-in for the Bot API, and updates are pushed to the server the receiver itself starts.
/// </summary>
public class WebhookMessageReceiverTests : TestBase
{
    /// <summary>
    /// The header Telegram echoes the configured secret token back in.
    /// </summary>
    private const string SecretHeader = "X-Telegram-Bot-Api-Secret-Token";

    /// <summary>
    /// The secret token the receiver under test is configured with.
    /// </summary>
    private const string SecretToken = "s3cret-token";

    /// <summary>
    /// Creates the fixture.
    /// </summary>
    /// <param name="outputHelper">The xunit output helper test logs are written to.</param>
    public WebhookMessageReceiverTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// An update pushed with the right secret token reaches the consumer.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Push_ValidSecretToken_DeliversUpdate()
    {
        // arrange
        await using var fixture = await WebhookFixture.StartAsync(this);

        // act
        var status = await fixture.PushAsync(SecretToken, UpdateJson(7));
        var update = await fixture.ReadOneAsync();

        // assert
        status.Is(HttpStatusCode.OK);
        update.Id.Is(7L);
        update.Message.NotNull().Text.Is("hi");
    }

    /// <summary>
    /// A push carrying the wrong secret token is refused and never reaches the consumer: this header is the
    /// only thing separating Telegram from anyone who learns the webhook address.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Push_InvalidSecretToken_IsRefusedAndDropped()
    {
        // arrange
        await using var fixture = await WebhookFixture.StartAsync(this);

        // act
        var status = await fixture.PushAsync("not-the-secret", UpdateJson(7));

        // assert
        status.Is(HttpStatusCode.Forbidden);
        fixture.Receiver.Updates.TryRead(out _).IsFalse("a forged update must not reach the consumer");
    }

    /// <summary>
    /// A push with no secret token header at all is refused the same way.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Push_MissingSecretToken_IsRefusedAndDropped()
    {
        // arrange
        await using var fixture = await WebhookFixture.StartAsync(this);

        // act
        var status = await fixture.PushAsync(null, UpdateJson(7));

        // assert
        status.Is(HttpStatusCode.Forbidden);
        fixture.Receiver.Updates.TryRead(out _).IsFalse("a request without the header must not reach the consumer");
    }

    /// <summary>
    /// A body that cannot be read as an update is reported and dropped: unlike polling there is no retry, so
    /// the only trace it leaves is the log line.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Push_MalformedPayload_IsReportedAndDropped()
    {
        // arrange
        await using var fixture = await WebhookFixture.StartAsync(this);

        // act
        var status = await fixture.PushAsync(SecretToken, "this is not json");

        // assert
        status.Is(HttpStatusCode.InternalServerError);
        fixture.Receiver.Updates.TryRead(out _).IsFalse("a payload that failed to parse must not be delivered");
        Logs.Any(x => x.Level >= LogLevel.Error).IsTrue("the dropped update must leave a trace");
    }

    /// <summary>
    /// A rejected setWebhook faults the update channel rather than leaving the host waiting for updates that
    /// can never arrive.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SetWebhookRejected_FaultsUpdatesChannel()
    {
        // arrange
        var (server, context) = RunApi((_, _) => new ApiReply("""{"ok":false,"description":"Unauthorized"}"""));
        await using var _ = server;
        var port = ReserveFreePort();

        // act
        await using var receiver = new WebhookMessageReceiver(
            Get<IServiceProvider>(),
            Configuration(port),
            context,
            Logger
        );

        // assert - the failure surfaces where the host reads, not as an unobserved background fault
        await Wrap.It(async () => await receiver.Updates.ReadAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Disposing the receiver completes the update channel, so a consumer stops instead of hanging.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Dispose_CompletesUpdatesChannel()
    {
        // arrange
        var fixture = await WebhookFixture.StartAsync(this);

        // act
        await fixture.DisposeAsync();

        // assert
        (await fixture.Receiver.Updates.WaitToReadAsync(TestContext.Current.CancellationToken)).IsFalse();
    }

    /// <summary>
    /// Disposing twice is not an error: teardown paths routinely run more than once.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Dispose_CalledTwice_DoesNotThrow()
    {
        // arrange
        var fixture = await WebhookFixture.StartAsync(this);

        // act
        await fixture.DisposeAsync();

        // assert - the second pass finds the work already done and returns quietly
        await fixture.DisposeAsync();
    }

    /// <summary>
    /// Builds the raw JSON of a text-message update.
    /// </summary>
    /// <param name="id">The update id to carry.</param>
    /// <returns>The update payload as Telegram would push it.</returns>
    private static string UpdateJson(long id) =>
        $$$"""
            {"update_id":{{{id}}},"message":{"message_id":1,"chat":{"id":42,"type":"private"},"date":1,"text":"hi"}}
            """;

    /// <summary>
    /// Builds a configuration pointing the webhook at a local port.
    /// </summary>
    /// <param name="port">The port the receiver's own server listens on.</param>
    /// <returns>The bot configuration.</returns>
    private static TelegramBotConfiguration Configuration(ushort port) =>
        new()
        {
            Token = "test-token",
            Webhook = new TelegramBotWebhookConfiguration
            {
                InternalPort = port,
                ExternalAddress = new Uri($"https://example.test/{port}"),
                SecretToken = SecretToken,
            },
        };

    /// <summary>
    /// Finds a port nothing is listening on. The receiver takes its port from configuration, so unlike the Bot
    /// API stand-in it cannot let the listener pick one for itself.
    /// </summary>
    /// <returns>A free local port.</returns>
    private static ushort ReserveFreePort()
    {
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = (ushort)((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();

        return port;
    }

    /// <summary>
    /// A started receiver together with the Bot API stand-in it registered its webhook with.
    /// </summary>
    private sealed class WebhookFixture : IAsyncDisposable
    {
        /// <summary>
        /// The receiver under test.
        /// </summary>
        public WebhookMessageReceiver Receiver { get; }

        /// <summary>
        /// The local Bot API stand-in that answered setWebhook.
        /// </summary>
        private readonly Net.Servers.Web.IServer _api;

        /// <summary>
        /// The address the receiver's own server listens on.
        /// </summary>
        private readonly string _url;

        /// <summary>
        /// Creates the fixture around an already-started receiver.
        /// </summary>
        /// <param name="receiver">The receiver under test.</param>
        /// <param name="api">The local Bot API stand-in that answered setWebhook.</param>
        /// <param name="url">The address the receiver's own server listens on.</param>
        private WebhookFixture(WebhookMessageReceiver receiver, Net.Servers.Web.IServer api, string url)
        {
            Receiver = receiver;
            _api = api;
            _url = url;
        }

        /// <summary>
        /// Starts a receiver whose setWebhook call succeeds, and waits until its server accepts connections.
        /// </summary>
        /// <param name="test">The test owning the fixture.</param>
        /// <returns>The started fixture.</returns>
        public static async Task<WebhookFixture> StartAsync(WebhookMessageReceiverTests test)
        {
            var (api, context) = test.RunApi((_, _) => new ApiReply("""{"ok":true,"result":true}"""));

            // the port is reserved by binding and releasing, so another listener can still take it in the
            // gap before the receiver binds. That race is in the fixture, not the code under test, so it is
            // retried on a fresh port rather than reported as a failure of the receiver
            for (var attempt = 1; ; attempt++)
            {
                var port = ReserveFreePort();
                var receiver = new WebhookMessageReceiver(
                    test.Get<IServiceProvider>(),
                    Configuration(port),
                    context,
                    test.Logger
                );

                if (await IsListeningAsync(port, receiver))
                    return new WebhookFixture(receiver, api, $"http://127.0.0.1:{port}/");

                await receiver.DisposeAsync();
                if (attempt == 3)
                {
                    await api.DisposeAsync();

                    throw new InvalidOperationException("webhook server did not start on three separate ports");
                }
            }
        }

        /// <summary>
        /// Pushes a raw body to the receiver's server, the way Telegram would.
        /// </summary>
        /// <param name="secretToken">The secret token to send, or null to omit the header entirely.</param>
        /// <param name="body">The raw request body.</param>
        /// <returns>The status code the receiver answered with.</returns>
        public async Task<HttpStatusCode> PushAsync(string? secretToken, string body)
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, _url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
            if (secretToken is not null)
                request.Headers.Add(SecretHeader, secretToken);

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            return response.StatusCode;
        }

        /// <summary>
        /// Reads a single update, bounded in time so a missing one fails the test instead of hanging it.
        /// </summary>
        /// <returns>The first update the receiver yields.</returns>
        public async Task<Integration.Shared.Domain.Update> ReadOneAsync()
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            return await Receiver.Updates.ReadAsync(cts.Token);
        }

        /// <summary>
        /// Stops the receiver and the Bot API stand-in.
        /// </summary>
        /// <returns>A task that completes once both have stopped.</returns>
        public async ValueTask DisposeAsync()
        {
            await Receiver.DisposeAsync();
            await _api.DisposeAsync();
        }

        /// <summary>
        /// Waits until the receiver's server accepts a connection, or gives up.
        /// </summary>
        /// <param name="port">The port to wait for.</param>
        /// <param name="receiver">The receiver being waited on, watched for an early failure.</param>
        /// <returns>Whether the server came up.</returns>
        private static async Task<bool> IsListeningAsync(ushort port, WebhookMessageReceiver receiver)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < TimeSpan.FromSeconds(5))
            {
                // the receiver registers the webhook before starting its server, and a failure there
                // completes the channel instead of ever listening — that is a real failure of the code under
                // test and is surfaced immediately, unlike a port that simply never came up
                // VSTHRD003: the receiver's completion is exactly the foreign signal worth observing here
#pragma warning disable VSTHRD003
                if (receiver.Updates.Completion.IsCompleted)
                    await receiver.Updates.Completion;
#pragma warning restore VSTHRD003

                try
                {
                    using var probe = new TcpClient();
                    await probe.ConnectAsync(IPAddress.Loopback, port, TestContext.Current.CancellationToken);

                    return true;
                }
                catch (SocketException)
                {
                    await Task.Delay(25, TestContext.Current.CancellationToken);
                }
            }

            return false;
        }
    }
}
