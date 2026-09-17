namespace Annium.Net.Sockets.Tests;

/// <summary>
/// The deadline every asynchronous test in this assembly carries.
/// </summary>
/// <remarks>
/// <para>
/// This assembly has hung twice: once on a CI runner, which reported the job as cancelled after 45
/// minutes and named <c>Annium.Net.Sockets.Tests</c> only in its orphan-process cleanup, and once
/// locally, where a full run took 25 minutes instead of two and the next one was clean. Neither told
/// anyone which test did not return, because a test with no deadline does not fail - it stops the run,
/// and a stopped run names nothing.
/// </para>
/// <para>
/// A minute is far more than any test here needs; the whole assembly runs in seconds. It is set that
/// high on purpose, so a loaded CI runner never trips it and the deadline stays a report of something
/// genuinely stuck rather than a source of flakes.
/// </para>
/// </remarks>
internal static class TestTimeout
{
    /// <summary>The deadline, in milliseconds.</summary>
    public const int Ms = 60_000;
}
