using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Annium.Testing.Containers.Tests;

/// <summary>
/// Pins the deadline that <see cref="ContainerExtensions"/> puts around starting a container.
/// </summary>
/// <remarks>
/// <para>
/// These run against a delegate rather than a container on purpose. What is worth pinning is that a start
/// which never completes fails in time and says what did not come up — and a docker daemon cannot be asked
/// to wedge a container on command, so a test that used one would either not reproduce the case at all or
/// would need the very hour this code exists to avoid.
/// </para>
/// <para>
/// Each carries a deadline of its own: a test of a deadline that hangs when the deadline stops working is
/// the failure mode being fixed, reproduced in the test suite.
/// </para>
/// </remarks>
public class ContainerExtensionsTests
{
    /// <summary>The deadline the tests give the code under test — short, since nothing here pulls an image.</summary>
    private static readonly TimeSpan _deadline = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// A start that never completes fails on the deadline, and names what failed to start.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = 30_000)]
    public async Task Deadline_FailsTheStartThatNeverCompletes()
    {
        // arrange
        var watch = Stopwatch.StartNew();

        // act
        var exception = await Wrap.It(async ValueTask () =>
                await ContainerExtensions.RunWithDeadlineAsync(
                    ct => Task.Delay(Timeout.Infinite, ct),
                    "quay.io/minio/minio:latest",
                    TestContext.Current.CancellationToken,
                    _deadline
                )
            )
            .ThrowsAsync<TimeoutException>();

        // assert - it failed on the deadline, not on something else that happened to throw
        watch.Elapsed.IsLess(TimeSpan.FromSeconds(10));

        // assert - and the message names the image, which is the whole point of catching it here rather
        // than letting the wait strategy report from inside Testcontainers
        exception.Message.Contains("quay.io/minio/minio:latest").IsTrue(exception.Message);
    }

    /// <summary>
    /// The token handed to the start operation is the one that carries the deadline, so an operation that
    /// honours it stops by itself rather than being abandoned still running.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = 30_000)]
    public async Task Deadline_ReachesTheStartOperation()
    {
        // arrange
        var observed = CancellationToken.None;

        // act
        await Wrap.It(async ValueTask () =>
                await ContainerExtensions.RunWithDeadlineAsync(
                    ct =>
                    {
                        observed = ct;

                        return Task.Delay(Timeout.Infinite, ct);
                    },
                    "image",
                    TestContext.Current.CancellationToken,
                    _deadline
                )
            )
            .ThrowsAsync<TimeoutException>();

        // assert
        observed.IsCancellationRequested.IsTrue("the start operation was left running past the deadline");
    }

    /// <summary>
    /// A caller who cancels gets cancellation, not a timeout.
    /// </summary>
    /// <remarks>
    /// The distinction earns its test: a cancelled run reported as a container failure sends whoever reads
    /// it to the docker daemon, which is fine, and nothing there is wrong.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = 30_000)]
    public async Task CallerCancellation_StaysCancellation()
    {
        // arrange
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cts.CancelAsync();

        // act, assert - the deadline here is long, so a timeout could only come from the wrong branch
        await Wrap.It(async ValueTask () =>
                await ContainerExtensions.RunWithDeadlineAsync(
                    ct => Task.Delay(Timeout.Infinite, ct),
                    "image",
                    cts.Token,
                    TimeSpan.FromMinutes(5)
                )
            )
            .ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// A start that completes in time completes, and is not turned into a failure by the deadline.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = 30_000)]
    public async Task Start_ThatCompletesInTime_Passes()
    {
        var started = false;

        await ContainerExtensions.RunWithDeadlineAsync(
            _ =>
            {
                started = true;

                return Task.CompletedTask;
            },
            "image",
            TestContext.Current.CancellationToken,
            _deadline
        );

        started.IsTrue();
    }
}
