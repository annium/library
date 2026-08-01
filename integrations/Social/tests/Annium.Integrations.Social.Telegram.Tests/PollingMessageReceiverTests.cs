using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Internal.Integration.Receivers;
using Annium.Testing;
using Xunit;

namespace Annium.Integrations.Social.Telegram.Tests;

/// <summary>
/// Tests for the polling receiver: which updates reach the consumer, how the poll offset advances,
/// and how failure and shutdown are surfaced.
/// </summary>
public class PollingMessageReceiverTests : TestBase
{
    public PollingMessageReceiverTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A message carrying no text (photo, sticker, …) is delivered instead of breaking the batch.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Poll_NonTextMessage_IsDelivered()
    {
        // arrange — regression: Message.Text was required, so a photo message made deserialization of
        // the whole batch fail and no update in it ever reached the handler
        var (server, context) = RunApi(
            (_, _) =>
                new ApiReply(
                    """
                    {"ok":true,"result":[{"update_id":1,"message":{"message_id":10,"chat":{"id":42,"type":"private"},
                    "date":1,"from":{"id":7,"is_bot":false,"first_name":"Ann"}}}]}
                    """
                )
        );
        await using var _ = server;

        // act
        await using var receiver = new PollingMessageReceiver(context, Logger);
        var update = await ReadOneAsync(receiver);

        // assert
        update.Id.Is(1L);
        update.Message.NotNull().Text.IsDefault();
        update.Message.NotNull().Chat.Id.Is(42L);
    }

    /// <summary>
    /// A channel post (no sender) is delivered instead of breaking the batch.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Poll_MessageWithoutFromOrUsername_IsDelivered()
    {
        // arrange — channel posts carry no "from", users without a public username carry no "username"
        var (server, context) = RunApi(
            (_, _) =>
                new ApiReply(
                    """
                    {"ok":true,"result":[{"update_id":5,"message":{"message_id":10,"chat":{"id":42,"type":"channel"},
                    "date":1,"text":"hi"}}]}
                    """
                )
        );
        await using var _ = server;

        // act
        await using var receiver = new PollingMessageReceiver(context, Logger);
        var update = await ReadOneAsync(receiver);

        // assert
        update.Message.NotNull().From.IsDefault();
        update.Message.NotNull().Text.Is("hi");
    }

    /// <summary>
    /// Received updates are confirmed by polling from the id past the highest one received.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Poll_AfterBatch_AdvancesOffsetPastLastUpdate()
    {
        // arrange
        var offsets = new ConcurrentQueue<string?>();
        var served = 0;
        var (server, context) = RunApi(
            (_, query) =>
            {
                offsets.Enqueue(query["offset"]);

                return Interlocked.Increment(ref served) == 1
                    ? new ApiReply(
                        """
                        {"ok":true,"result":[{"update_id":3,"message":{"message_id":1,"chat":{"id":1,"type":"private"},"date":1,"text":"a"}},
                        {"update_id":9,"message":{"message_id":2,"chat":{"id":1,"type":"private"},"date":1,"text":"b"}}]}
                        """
                    )
                    : new ApiReply("""{"ok":true,"result":[]}""");
            }
        );
        await using var _ = server;

        // act
        await using var receiver = new PollingMessageReceiver(context, Logger);
        await ReadOneAsync(receiver);
        await ReadOneAsync(receiver);
        await WaitUntilAsync(() => offsets.Count >= 2);

        // assert — updates are confirmed by asking for the one past the highest id received
        offsets.TryDequeue(out var first).IsTrue();
        first.Is("0");
        offsets.TryDequeue(out var second).IsTrue();
        second.Is("10");
    }

    /// <summary>
    /// A failing getUpdates call is retried on a pause rather than in a hot loop.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Poll_FailedResponse_RetriesWithoutSpinning()
    {
        // arrange — regression: a non-OK response was logged and retried immediately, turning the loop
        // into an unthrottled hammer against the Bot API
        var calls = 0;
        var (server, context) = RunApi(
            (_, _) =>
            {
                Interlocked.Increment(ref calls);

                return new ApiReply("""{"ok":false,"description":"Unauthorized"}""");
            }
        );
        await using var _ = server;

        // act
        await using var receiver = new PollingMessageReceiver(context, Logger);
        await WaitUntilAsync(() => calls >= 1);
        await Task.Delay(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        // assert — with a 5s pause between failures at most a couple of calls fit into this window
        (Volatile.Read(ref calls) <= 2).IsTrue($"expected at most 2 calls in 1s, got {Volatile.Read(ref calls)}");
        Logs.Any(x => x.Message.Contains("Unauthorized")).IsTrue("failure description must be logged");
    }

    /// <summary>
    /// Disposing the receiver completes the update channel, so consumers stop instead of hanging.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Dispose_CompletesUpdatesChannel()
    {
        // arrange — regression: the channel was left open, so TelegramBotHost.RunAsync awaited an
        // update that could never arrive and the bot hung silently after the receiver stopped
        var (server, context) = RunApi((_, _) => new ApiReply("""{"ok":true,"result":[]}"""));
        await using var _ = server;
        var receiver = new PollingMessageReceiver(context, Logger);

        // act
        await receiver.DisposeAsync();

        // assert
        receiver.Updates.Completion.IsCompleted.IsTrue();
        (await receiver.Updates.WaitToReadAsync(TestContext.Current.CancellationToken)).IsFalse();
    }

    /// <summary>
    /// Reads a single update, bounded in time: an update that never arrives must fail the test
    /// rather than hang the whole run.
    /// </summary>
    /// <param name="receiver">The receiver to read from.</param>
    /// <returns>The first update the receiver yields.</returns>
    private static async Task<Integration.Shared.Domain.Update> ReadOneAsync(PollingMessageReceiver receiver)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        return await receiver.Updates.ReadAsync(cts.Token);
    }

    /// <summary>
    /// Polls until the condition holds, failing the test if it does not within 10 seconds.
    /// </summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var sw = Stopwatch.StartNew();
        while (!condition() && sw.Elapsed < TimeSpan.FromSeconds(10))
            await Task.Delay(25, TestContext.Current.CancellationToken);

        condition().IsTrue("condition was not met within 10s");
    }
}
