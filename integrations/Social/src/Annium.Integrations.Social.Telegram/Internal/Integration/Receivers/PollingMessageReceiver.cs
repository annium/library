using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Integration.Receivers;
using Annium.Integrations.Social.Telegram.Integration.Shared.Domain;
using Annium.Logging;
using Annium.Net.Http;

namespace Annium.Integrations.Social.Telegram.Internal.Integration.Receivers;

internal sealed class PollingMessageReceiver : ITelegramMessageReceiver, IAsyncDisposable, ILogSubject
{
    public ILogger Logger { get; }
    public ChannelReader<Update> Updates { get; }
    private readonly CancellationTokenSource _cts;
    private readonly Task _task;

    public PollingMessageReceiver(ApiContext context, ILogger logger)
    {
        Logger = logger;
        _cts = new CancellationTokenSource();

        var channel = Channel.CreateUnbounded<Update>();
        Updates = channel.Reader;

        _task = Task.Run(Poll(context, channel.Writer, _cts.Token));
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
#pragma warning disable VSTHRD003
        await _task;
#pragma warning restore VSTHRD003
    }

    private Func<Task> Poll(ApiContext context, ChannelWriter<Update> writer, CancellationToken ct) =>
        async () =>
        {
            this.Trace("start");

            var lastUpdateId = -1L;

            try
            {
                while (await writer.WaitToWriteAsync(ct))
                {
                    this.Trace("start polling");

                    while (!ct.IsCancellationRequested)
                    {
                        var response = await context
                            .Http.Get("getUpdates")
                            .Param("offset", lastUpdateId + 1)
                            .Param("timeout", 60)
                            .WithLogFrom(this)
                            .Timeout(TimeSpan.FromMinutes(5))
                            .AsAsync<Response<IReadOnlyList<Update>>>(ct);

                        if (response is null)
                            this.Trace("failed to parse response");
                        else if (response.Ok)
                        {
                            foreach (var update in response.Result)
                            {
                                lastUpdateId = Math.Max(lastUpdateId, update.Id);
                                await writer.WriteAsync(update, ct);
                            }
                        }
                        else
                            this.Error<string>("getUpdates failed: {description}", response.Description);
                    }

                    this.Trace("done polling");
                }
            }
            catch (OperationCanceledException)
            {
                this.Trace("polling canceled");
            }
            catch (Exception e)
            {
                this.Error(e);
            }

            this.Trace("done");
        };
}
