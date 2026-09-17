using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;

namespace Annium.Testing.Containers;

/// <summary>
/// Starts a Testcontainers container under a deadline, so a container that never becomes ready fails
/// the test instead of stalling the run.
/// </summary>
/// <remarks>
/// <para>
/// Testcontainers waits an hour for readiness by default. That is not a number anyone picked for a test
/// suite: a container wedged by a bad image, a docker daemon out of disk or a registry that stopped
/// answering holds the whole group for an hour and then reports a <c>TimeoutException</c> from inside the
/// wait strategy, blamed on whichever test happened to trigger the start. It has happened here — a MinIO
/// container took 01:00:01 to fail, and the run never reached the four groups after it.
/// </para>
/// <para>
/// A deadline does not make a wedged container work. It makes the failure arrive while someone is still
/// watching, naming the image that did not come up, which is the difference between a diagnosable run and
/// a run that looks hung.
/// </para>
/// <para>
/// The deadline covers the pull as well as the readiness wait, since <c>StartAsync</c> does both and they
/// cannot be timed apart from out here. That is why the default is minutes rather than seconds: a cold
/// pull of a Kafka image is a legitimate reason to take a while, and a deadline that fired on it would
/// simply be a flakier suite.
/// </para>
/// </remarks>
public static class ContainerExtensions
{
    /// <summary>
    /// Gets the deadline used when a caller does not name one — long enough for a cold image pull, short
    /// enough that a wedged container is a failure within the span of a coffee.
    /// </summary>
    public static TimeSpan DefaultStartupTimeout { get; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Starts the container, failing with a <see cref="TimeoutException"/> if it is not up in time.
    /// </summary>
    /// <param name="container">The container to start.</param>
    /// <param name="ct">The test's cancellation token, honoured as its own reason to stop.</param>
    /// <param name="timeout">The deadline; <see cref="DefaultStartupTimeout"/> when omitted.</param>
    /// <returns>A task that completes when the container is ready.</returns>
    public static Task StartWithDeadlineAsync(
        this IContainer container,
        CancellationToken ct = default,
        TimeSpan? timeout = null
    ) => RunWithDeadlineAsync(container.StartAsync, container.Image.FullName, ct, timeout);

    /// <summary>
    /// Runs a start operation under a deadline, translating the deadline into a message that names what
    /// failed to come up.
    /// </summary>
    /// <remarks>
    /// Exposed separately from the container overload so the deadline itself can be tested without a
    /// docker daemon: the behaviour worth pinning is that the call returns in time and says what timed
    /// out, and a delegate that never completes exercises exactly that.
    /// </remarks>
    /// <param name="startAsync">The start operation, which must honour the token it is given.</param>
    /// <param name="what">What is being started, named in the failure message.</param>
    /// <param name="ct">The caller's cancellation token, honoured as its own reason to stop.</param>
    /// <param name="timeout">The deadline; <see cref="DefaultStartupTimeout"/> when omitted.</param>
    /// <returns>A task that completes when the operation does.</returns>
    public static async Task RunWithDeadlineAsync(
        Func<CancellationToken, Task> startAsync,
        string what,
        CancellationToken ct = default,
        TimeSpan? timeout = null
    )
    {
        var deadline = timeout ?? DefaultStartupTimeout;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(deadline);

        try
        {
            await startAsync(cts.Token);
        }
        // only the deadline is translated: a token the caller cancelled is the caller's own business and
        // has to keep surfacing as cancellation, or a cancelled run reads as a container failure
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"{what} did not start within {deadline:g}. Check the docker daemon, the image and the registry - "
                    + "the container was still not ready when the deadline passed."
            );
        }
    }
}
