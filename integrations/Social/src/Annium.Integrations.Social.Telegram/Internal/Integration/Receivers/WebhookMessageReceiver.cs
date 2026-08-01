using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Integration.Receivers;
using Annium.Integrations.Social.Telegram.Integration.Shared.Domain;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Net.Servers.Web;
using Annium.Serialization.Abstractions;
using Annium.Threading;

namespace Annium.Integrations.Social.Telegram.Internal.Integration.Receivers;

/// <summary>
/// Receives Telegram updates by registering a webhook with <c>setWebhook</c> and running an HTTP server that
/// Telegram pushes updates to.
/// </summary>
internal sealed class WebhookMessageReceiver : ITelegramMessageReceiver, IAsyncDisposable, ILogSubject
{
    /// <summary>
    /// The service provider used to build the HTTP server that accepts webhook pushes.
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// The logger used to record webhook setup, lifecycle, and failure events.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the channel reader that yields updates received from the webhook.
    /// </summary>
    public ChannelReader<Update> Updates { get; }

    /// <summary>
    /// Cancels the background webhook server task on dispose.
    /// </summary>
    private readonly CancellationTokenSource _cts;

    /// <summary>
    /// The background task running the webhook server.
    /// </summary>
    private readonly Task _task;

    public WebhookMessageReceiver(
        IServiceProvider sp,
        TelegramBotConfiguration config,
        ApiContext context,
        ILogger logger
    )
    {
        _sp = sp;
        Logger = logger;

        var cfg = config.Webhook;
        if (cfg is null)
            throw new InvalidOperationException("Webhook is not configured");

        var externalAddress = cfg.ExternalAddress;
        if (externalAddress is null)
            throw new InvalidOperationException("Webhook external address is not configured");

        var secretToken = cfg.SecretToken;
        if (string.IsNullOrWhiteSpace(secretToken))
            throw new InvalidOperationException("Webhook secret token is not configured");

        _cts = new CancellationTokenSource();

        var channel = Channel.CreateUnbounded<Update>();
        Updates = channel.Reader;

        _task = Task.Run(Start(cfg.InternalPort, externalAddress, secretToken, context, channel.Writer, _cts.Token));
    }

    /// <summary>
    /// Cancels the webhook server and waits for it to shut down before returning.
    /// </summary>
    /// <returns>A task that completes once the server has stopped.</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        this.Trace("cancel run");
        await _cts.CancelAsync();

        this.Trace("await run complete");
#pragma warning disable VSTHRD003
        await _task;
#pragma warning restore VSTHRD003

        this.Trace("done");
    }

    /// <summary>
    /// Builds the delegate that registers the webhook via <c>setWebhook</c> and runs an HTTP server on
    /// <paramref name="internalPort"/> to receive pushed updates until canceled, completing <paramref name="writer"/>
    /// on shutdown or failure.
    /// </summary>
    /// <param name="internalPort">The local port the webhook HTTP server listens on.</param>
    /// <param name="externalAddress">The externally reachable URL registered with Telegram as the webhook target.
    /// </param>
    /// <param name="secretToken">The secret token Telegram must echo back on each webhook request.</param>
    /// <param name="context">The API context used to call <c>setWebhook</c>.</param>
    /// <param name="writer">The channel writer updates are published to.</param>
    /// <param name="ct">The token that stops the server.</param>
    /// <returns>The delegate to run as the background webhook-server task.</returns>
    private Func<Task> Start(
        ushort internalPort,
        Uri externalAddress,
        string secretToken,
        ApiContext context,
        ChannelWriter<Update> writer,
        CancellationToken ct
    ) =>
        async () =>
        {
            this.Trace("start");

            try
            {
                this.Trace("Set webhook address: {address}", externalAddress);
                var setWebhookResponse = await context
                    .Http.Get("setWebhook")
                    .Param("url", externalAddress)
                    .Param("secret_token", secretToken)
                    .WithLogFrom(this)
                    .AsAsync<Response<bool>>(ct);

                if (setWebhookResponse is null)
                    throw new InvalidOperationException("Failed to parse setWebhook response");

                if (!setWebhookResponse.Ok)
                    throw new InvalidOperationException($"Webhook set failure: {setWebhookResponse.Description}");

                this.Trace("Start server at port {port}", internalPort);
                await using var server = ServerBuilder
                    .New(_sp, port: internalPort)
                    .WithHttpHandler(new WebHookHandler(secretToken, context.Serializer, writer, Logger))
                    .Start();

                await ct;

                writer.TryComplete();
            }
            catch (OperationCanceledException)
            {
                this.Trace("webhook canceled");
                writer.TryComplete();
            }
            catch (Exception e)
            {
                // setWebhook failure used to fault this background task unobserved: the bot silently
                // received nothing, and DisposeAsync rethrew the fault during teardown. Completing the
                // channel with the error surfaces it to TelegramBotHost.RunAsync instead
                this.Error(e);
                writer.TryComplete(e);
            }

            this.Trace("done");
        };
}

/// <summary>
/// HTTP handler that validates the Telegram secret token header and forwards each pushed update to a channel.
/// </summary>
file class WebHookHandler : IHttpHandler, ILogSubject
{
    /// <summary>
    /// The logger used to record invalid requests and processing failures.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The secret token expected in the <c>X-Telegram-Bot-Api-Secret-Token</c> header of each request.
    /// </summary>
    private readonly string _secretToken;

    /// <summary>
    /// The serializer used to deserialize the update payload from the request body.
    /// </summary>
    private readonly ISerializer<Stream> _serializer;

    /// <summary>
    /// The channel writer incoming updates are published to.
    /// </summary>
    private readonly ChannelWriter<Update> _writer;

    public WebHookHandler(
        string secretToken,
        ISerializer<Stream> serializer,
        ChannelWriter<Update> writer,
        ILogger logger
    )
    {
        Logger = logger;
        _secretToken = secretToken;
        _serializer = serializer;
        _writer = writer;
    }

    /// <summary>
    /// Validates the request's secret token, deserializes and forwards the update, and writes an HTTP status
    /// reflecting the outcome (200 on success, 403 on an invalid token, 500 on failure).
    /// </summary>
    /// <param name="ctx">The incoming HTTP listener context.</param>
    /// <param name="ct">The cancellation token for the request.</param>
    /// <returns>A task that completes once the response has been written.</returns>
    public async Task HandleAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        var statusCode = HttpStatusCode.OK;
        try
        {
            // verify secret token; compared in fixed time, since a length-dependent comparison of a
            // shared secret against attacker-controlled input is measurable over repeated requests
            var headerToken = ctx.Request.Headers.Get("X-Telegram-Bot-Api-Secret-Token");
            if (!IsSecretTokenValid(headerToken))
            {
                this.Warn<string?>("Received request with invalid secret token: {token}", headerToken);
                statusCode = HttpStatusCode.Forbidden;
                return;
            }

            var update = _serializer.Deserialize<Update?>(ctx.Request.InputStream);
            if (update is not null)
                await _writer.WriteAsync(update, CancellationToken.None);
            else
                this.Warn("Failed to parse update");
        }
        catch (Exception e)
        {
            statusCode = HttpStatusCode.InternalServerError;
            this.Error(e);
        }
        finally
        {
            ctx.Response.StatusCode = (int)statusCode;
            ctx.Response.Close();
        }
    }

    /// <summary>
    /// Compares the given header value against the configured secret token using a fixed-time comparison, to avoid
    /// leaking the secret via timing differences.
    /// </summary>
    /// <param name="headerToken">The secret token value taken from the request header, if present.</param>
    /// <returns><see langword="true"/> if the header matches the configured secret token; otherwise,
    /// <see langword="false"/>.</returns>
    private bool IsSecretTokenValid(string? headerToken)
    {
        if (headerToken is null)
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(headerToken),
            Encoding.UTF8.GetBytes(_secretToken)
        );
    }
}
