using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Logging;

namespace Annium.Execution.Flow;

/// <summary>
/// Repeats a piece of work until it reports itself done, backing off between attempts that achieve nothing.
/// </summary>
/// <remarks>
/// Written because the hand-rolled version - <c>while (!ready) await Attempt();</c> - is a hot loop the
/// moment the thing it waits on starts failing rather than answering. It spins as fast as the failures come
/// back and logs a line per turn: one run of it produced 3.3 million lines in 1.4 GB. Against a rate-limited
/// service it does worse than waste a core, since the requests it keeps making are what holds the limit
/// closed.
/// </remarks>
public static class Repeat
{
    /// <summary>
    /// Attempts the work until <paramref name="isDone"/> holds, waiting longer after each attempt that
    /// achieves nothing and giving up on one that reports itself unfixable.
    /// </summary>
    /// <remarks>
    /// Reporting is the caller's to leave alone: an attempt should say what went wrong through its
    /// <see cref="Attempt.Message"/> rather than log it, because logging per attempt is the flood this
    /// exists to stop. What lands in the log is the first stall, the recovery, and the giving up.
    /// </remarks>
    /// <param name="isDone">Whether the work is finished; checked before every attempt.</param>
    /// <param name="attemptAsync">One attempt at the work.</param>
    /// <param name="config">How long to wait between fruitless attempts.</param>
    /// <param name="log">The subject the progress of the repetition is logged against.</param>
    /// <param name="subject">What is being prepared, for the log.</param>
    /// <param name="ct">Cancels the repetition, including a wait already in progress.</param>
    /// <param name="waitAsync">
    /// How to wait, defaulting to <see cref="Task.Delay(TimeSpan, CancellationToken)"/>. A test passes one
    /// that records what it was asked for and returns at once.
    /// </param>
    /// <returns>A successful result once the work is done, or the failure that stopped it.</returns>
    /// <remarks>
    /// <para>
    /// <b>Why the wait is a parameter.</b> The pacing is the whole point of this helper, and it cannot be
    /// asserted by measuring: the first wait is tens of milliseconds, which is inside the noise a loaded
    /// machine adds to a timer. The test that tried failed about once in four full-suite runs, with the
    /// short wait measured at eleven times its nominal length - it was comparing scheduling noise against a
    /// capped wait and calling the difference backoff.
    /// </para>
    /// <para>
    /// Handed the wait instead, a test states the sequence the code chose - initial, each growth, the cap,
    /// and the reset after progress - exactly, in microseconds and without a clock. Production passes
    /// nothing and gets <see cref="Task.Delay(TimeSpan, CancellationToken)"/>.
    /// </para>
    /// </remarks>
    public static async Task<IResult> UntilAsync(
        Func<bool> isDone,
        Func<CancellationToken, ValueTask<Attempt>> attemptAsync,
        RepeatConfig config,
        ILogSubject log,
        string subject,
        CancellationToken ct,
        Func<TimeSpan, CancellationToken, Task>? waitAsync = null
    )
    {
        waitAsync ??= Task.Delay;

        var delay = config.InitialDelay;
        var stalls = 0;

        while (!isDone())
        {
            if (ct.IsCancellationRequested)
                return Result.Create().Error($"{subject}: cancelled");

            var attempt = await attemptAsync(ct);

            if (attempt.Outcome is AttemptOutcome.Failed)
            {
                log.Error<string, string>("{subject}: giving up - {message}", subject, attempt.Message);

                return Result.Create().Error($"{subject}: {attempt.Message}");
            }

            if (attempt.Outcome is AttemptOutcome.Progressed)
            {
                if (stalls > 0)
                {
                    log.Info<string, int>(
                        "{subject}: progressing again after {stalls} stalled attempt(s)",
                        subject,
                        stalls
                    );
                    stalls = 0;
                    delay = config.InitialDelay;
                }

                continue;
            }

            // stalled. Only the first one is announced: an upstream that is down stays down for as long as it
            // stays down, and saying so once a second is how a log reaches a gigabyte
            stalls++;
            if (stalls == 1)
                log.Warn<string, string>("{subject}: stalled - {message}, retrying", subject, attempt.Message);

            try
            {
                await waitAsync(delay, ct);
            }
            catch (OperationCanceledException)
            {
                return Result.Create().Error($"{subject}: cancelled");
            }

            delay = Min(delay * config.Factor, config.MaxDelay);
        }

        if (stalls > 0)
            log.Info<string, int>("{subject}: ready after {stalls} stalled attempt(s)", subject, stalls);

        return Result.Create();
    }

    /// <summary>
    /// Returns the shorter of two spans.
    /// </summary>
    /// <param name="a">The first span.</param>
    /// <param name="b">The second span.</param>
    /// <returns>The shorter span.</returns>
    private static TimeSpan Min(TimeSpan a, TimeSpan b) => a < b ? a : b;
}
