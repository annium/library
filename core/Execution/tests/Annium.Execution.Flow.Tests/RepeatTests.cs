using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Execution.Flow.Tests;

/// <summary>
/// Pins how <see cref="Repeat.UntilAsync"/> paces attempts: straight through while the work progresses,
/// waiting and growing the wait while it does not, and stopping outright on a failure repeating will not fix.
/// </summary>
public class RepeatTests : TestBase
{
    /// <summary>
    /// A repetition quick enough for a test to sit through several stalls, but whose waits are still spaced
    /// well clear of the system timer's resolution - measured any tighter, growth is lost in the noise.
    /// </summary>
    private static readonly RepeatConfig _fast = new(TimeSpan.FromMilliseconds(25), TimeSpan.FromMilliseconds(400), 4);

    /// <summary>
    /// Initializes a new instance of the <see cref="RepeatTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper used to capture test logs.</param>
    public RepeatTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Work that reports itself done is never attempted at all.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task AlreadyDone_IsNotAttempted()
    {
        // arrange
        var attempts = 0;

        // act
        var result = await Repeat.UntilAsync(
            () => true,
            _ =>
            {
                attempts++;
                return ValueTask.FromResult(Attempt.Progressed);
            },
            _fast,
            this,
            "subject",
            CancellationToken.None
        );

        // assert
        result.HasErrors.IsFalse();
        attempts.Is(0);
    }

    /// <summary>
    /// Attempts repeat until the work reports itself done, and the result is successful.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Progress_RepeatsUntilDone()
    {
        // arrange
        var attempts = 0;

        // act
        var result = await Repeat.UntilAsync(
            () => attempts >= 3,
            _ =>
            {
                attempts++;
                return ValueTask.FromResult(Attempt.Progressed);
            },
            _fast,
            this,
            "subject",
            CancellationToken.None
        );

        // assert
        result.HasErrors.IsFalse();
        attempts.Is(3);
    }

    /// <summary>
    /// A failure that repeating will not fix stops the repetition at once, with the failure's own message.
    /// Left to repeat, a malformed request is malformed every time - and against an exchange, each repeat
    /// costs rate limit for an answer that cannot change.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Failure_StopsImmediately()
    {
        // arrange
        var attempts = 0;

        // act
        var result = await Repeat.UntilAsync(
            () => false,
            _ =>
            {
                attempts++;
                return ValueTask.FromResult(Attempt.Failed("bad request"));
            },
            _fast,
            this,
            "subject",
            CancellationToken.None
        );

        // assert
        attempts.Is(1);
        result.HasErrors.IsTrue();
        result.PlainErrors.Has(1);
    }

    /// <summary>
    /// The waits between fruitless attempts start at the configured one, grow by the configured factor, and
    /// stop growing at the configured ceiling.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Stated as the sequence the code asked for, not as the time that passed. The version that measured
    /// failed about once in four full-suite runs: its short wait, nominally 25ms, was recorded at 287ms
    /// under the load of a dozen parallel test processes, and against a wait already capped at 400ms the
    /// ratio it asserted could not hold. Nothing was wrong with the pacing - the instrument was a clock on
    /// a busy machine.
    /// </para>
    /// <para>
    /// Handed the wait, the test pins more than the old one could: the initial value, the factor, and the
    /// ceiling, each exactly.
    /// </para>
    /// </remarks>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Stalls_AreSpacedOutAndBackOff()
    {
        // arrange
        var waits = new List<TimeSpan>();
        var attempts = 0;
        const int stallCount = 4;

        // act
        var result = await Repeat.UntilAsync(
            () => attempts > stallCount,
            _ =>
            {
                attempts++;

                return ValueTask.FromResult(Attempt.Stalled("upstream is down"));
            },
            _fast,
            this,
            "subject",
            CancellationToken.None,
            (delay, _) =>
            {
                waits.Add(delay);

                return Task.CompletedTask;
            }
        );

        // assert - 25ms, four times that, then the 400ms ceiling, which the rest stay at.
        //
        // Five waits for five stalled attempts, not four: the wait comes after the attempt and the
        // readiness check comes before the next one, so the attempt that satisfies the predicate still
        // waits before the loop notices. Visible here and not in the version that measured, which asserted
        // over gaps between attempts and so could not count the last one at all
        result.HasErrors.IsFalse();
        waits.Has(stallCount + 1);
        waits[0].Is(TimeSpan.FromMilliseconds(25));
        waits[1].Is(TimeSpan.FromMilliseconds(100));
        waits[2].Is(TimeSpan.FromMilliseconds(400));
        waits[3].Is(TimeSpan.FromMilliseconds(400), "the wait grew past the configured ceiling");
        waits[4].Is(TimeSpan.FromMilliseconds(400));
    }

    /// <summary>
    /// A stall that gives way to progress resets the wait, so a brief outage does not leave a healthy
    /// upstream being polled at the outage's pace.
    /// </summary>
    /// <remarks>
    /// This one could only ever have been asserted as an upper bound - the wait after a recovery is
    /// <i>short</i> - which is the direction scheduling noise moves things in. It is now the value itself.
    /// </remarks>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ProgressAfterStalls_ResetsTheWait()
    {
        // arrange
        var waits = new List<TimeSpan>();
        var attempts = 0;

        // act - stall three times, then progress, then stall once more
        var result = await Repeat.UntilAsync(
            () => attempts >= 6,
            _ =>
            {
                attempts++;

                return ValueTask.FromResult(attempts == 4 ? Attempt.Progressed : Attempt.Stalled("down"));
            },
            _fast,
            this,
            "subject",
            CancellationToken.None,
            (delay, _) =>
            {
                waits.Add(delay);

                return Task.CompletedTask;
            }
        );

        // assert - grown to the ceiling before the recovery, back to the initial wait after it
        result.HasErrors.IsFalse();
        waits.Has(5);
        waits[2].Is(TimeSpan.FromMilliseconds(400));
        waits[3].Is(TimeSpan.FromMilliseconds(25), "progress did not reset the wait");
        waits[4].Is(TimeSpan.FromMilliseconds(100), "the wait did not start growing again after the reset");
    }

    /// <summary>
    /// The default wait is a real one: left to itself, the repetition actually sits out its configured
    /// delays rather than spinning.
    /// </summary>
    /// <remarks>
    /// The one thing the tests above cannot say, since they replace the waiting. Asserted as a lower bound,
    /// which is the only direction a clock on a loaded machine cannot break: noise makes a wait longer.
    /// </remarks>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task TheDefaultWait_ActuallyElapses()
    {
        // arrange
        var attempts = 0;
        var watch = Stopwatch.StartNew();

        // act - two stalls, so the configured waits are 25ms and 100ms
        var result = await Repeat.UntilAsync(
            () => attempts > 2,
            _ =>
            {
                attempts++;

                return ValueTask.FromResult(Attempt.Stalled("down"));
            },
            _fast,
            this,
            "subject",
            CancellationToken.None
        );

        // assert
        result.HasErrors.IsFalse();
        (watch.ElapsedMilliseconds >= 100).IsTrue(
            $"the repetition did not wait: {watch.ElapsedMilliseconds}ms for waits configured at 25ms + 100ms"
        );
    }

    /// <summary>
    /// Cancellation ends the repetition, including a wait already under way - a host being shut down must
    /// not have to sit out a backoff measured in minutes.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Cancellation_EndsTheRepetition()
    {
        // arrange
        using var cts = new CancellationTokenSource();
        var attempts = 0;
        var slow = new RepeatConfig(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1), 2);

        // act
        var watch = Stopwatch.StartNew();
        var task = Repeat.UntilAsync(
            () => false,
            _ =>
            {
                attempts++;
                return ValueTask.FromResult(Attempt.Stalled("down"));
            },
            slow,
            this,
            "subject",
            cts.Token
        );

        await Expect.ToAsync(() => attempts.Is(1));
        await cts.CancelAsync();
        var result = await task;
        watch.Stop();

        // assert
        result.HasErrors.IsTrue();
        (watch.Elapsed < TimeSpan.FromSeconds(20)).IsTrue($"cancellation waited out the backoff: {watch.Elapsed}");
    }
}
