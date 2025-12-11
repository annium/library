using System;
using System.IO;
using System.Net;
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

internal sealed class WebhookMessageReceiver : ITelegramMessageReceiver, IAsyncDisposable, ILogSubject
{
    private readonly IServiceProvider _sp;
    public ILogger Logger { get; }
    public ChannelReader<Update> Updates { get; }
    private readonly CancellationTokenSource _cts;
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

            this.Trace("Set webhook address: {address}", externalAddress);
            var setWebhookResponse = await context
                .Http.Get("setWebhook")
                .Param("url", externalAddress)
                .Param("secret_token", secretToken)
                .WithLogFrom(this)
                .AsAsync<Response<bool>>(ct);

            if (setWebhookResponse is null)
                throw new Exception("Failed to set webhook");

            if (!setWebhookResponse.Ok)
                throw new Exception($"Webhook set failure: {setWebhookResponse.Description}");

            this.Trace("Start server at port {port}", internalPort);
            await using var server = ServerBuilder
                .New(_sp, port: internalPort)
                .WithHttpHandler(new WebHookHandler(secretToken, context.Serializer, writer, Logger))
                .Start();

            await ct;

            this.Trace("done");
        };
}

file class WebHookHandler : IHttpHandler, ILogSubject
{
    public ILogger Logger { get; }
    private readonly string _secretToken;
    private readonly ISerializer<Stream> _serializer;
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

    public async Task HandleAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        var statusCode = HttpStatusCode.OK;
        try
        {
            // verify secret token
            var headerToken = ctx.Request.Headers.Get("X-Telegram-Bot-Api-Secret-Token");
            if (headerToken != _secretToken)
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
}
