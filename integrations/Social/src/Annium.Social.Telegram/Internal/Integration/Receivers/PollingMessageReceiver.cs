using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Social.Telegram.Integration.Receivers;
using Annium.Social.Telegram.Integration.Shared.Domain;

namespace Annium.Social.Telegram.Internal.Integration.Receivers;

/// <summary>
/// Receives Telegram updates by continuously long-polling the <c>getUpdates</c> endpoint on a background task,
/// confirming previously received updates via the <c>offset</c> parameter on each subsequent poll.
/// </summary>
internal sealed class PollingMessageReceiver : ITelegramMessageReceiver, IAsyncDisposable, ILogSubject
{
    /// <summary>
    /// Pause between failed getUpdates calls. Without it a permanently failing call (revoked token,
    /// unparseable batch) turns the loop into a hot retry against the Telegram API.
    /// </summary>
    private static readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The logger used to record polling failures and lifecycle events.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the channel reader that yields updates received from <c>getUpdates</c>.
    /// </summary>
    public ChannelReader<Update> Updates { get; }

    /// <summary>
    /// Cancels the background poll loop on dispose.
    /// </summary>
    private readonly CancellationTokenSource _cts;

    /// <summary>
    /// The background task running the poll loop.
    /// </summary>
    private readonly Task _task;

    /// <summary>
    /// Creates the receiver and starts its background poll loop.
    /// </summary>
    /// <param name="context">The API context used to poll for updates.</param>
    /// <param name="logger">The logger used to trace the poll loop.</param>
    public PollingMessageReceiver(ApiContext context, ILogger logger)
    {
        Logger = logger;
        _cts = new CancellationTokenSource();

        var channel = Channel.CreateUnbounded<Update>();
        Updates = channel.Reader;

        _task = Task.Run(Poll(context, channel.Writer, _cts.Token));
    }

    /// <summary>
    /// Cancels the background poll loop and waits for it to finish before returning.
    /// </summary>
    /// <returns>A task that completes once the poll loop has stopped.</returns>
    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        // VSTHRD003: _task is this receiver's own background loop, drained here so disposal does not
        // return before the loop has actually stopped
#pragma warning disable VSTHRD003
        await _task;
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Builds the delegate that runs the polling loop: repeatedly calls <c>getUpdates</c> with an incrementing
    /// <c>offset</c>, writes received updates to <paramref name="writer"/>, retries after <see cref="_retryDelay"/>
    /// on failure, and completes the writer when canceled or when the loop faults.
    /// </summary>
    /// <param name="context">The API context used to call <c>getUpdates</c>.</param>
    /// <param name="writer">The channel writer updates are published to.</param>
    /// <param name="ct">The token that stops the loop.</param>
    /// <returns>The delegate to run as the background polling task.</returns>
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
                            .WithRedactedLogFrom(this)
                            .Timeout(TimeSpan.FromMinutes(5))
                            .AsAsync<Response<IReadOnlyList<Update>>>(ct);

                        if (response is null)
                        {
                            this.Error("failed to parse getUpdates response");
                            await Task.Delay(_retryDelay, ct);
                        }
                        else if (response.Ok)
                        {
                            foreach (var update in response.Result)
                            {
                                lastUpdateId = Math.Max(lastUpdateId, update.Id);
                                await writer.WriteAsync(update, ct);
                            }
                        }
                        else
                        {
                            this.Error<string>("getUpdates failed: {description}", response.Description);
                            await Task.Delay(_retryDelay, ct);
                        }
                    }

                    this.Trace("done polling");
                }

                // the loop only ends on cancellation — complete without error so that consumers
                // awaiting Updates observe a clean shutdown instead of blocking forever
                writer.TryComplete();
            }
            catch (OperationCanceledException)
            {
                this.Trace("polling canceled");
                writer.TryComplete();
            }
            catch (Exception e)
            {
                this.Error(e);

                // hand the fault to the reader: TelegramBotHost.RunAsync otherwise awaits an
                // update that can never arrive, and the bot hangs with no diagnostic
                writer.TryComplete(e);
            }

            this.Trace("done");
        };
}
