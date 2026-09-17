namespace Annium.Net.WebSockets.Tests;

/// <summary>
/// The deadline every asynchronous test in this assembly carries.
/// </summary>
/// <remarks>
/// <para>
/// The hangs were observed in the sibling assembly, <c>Annium.Net.Sockets.Tests</c> — twice: on a CI
/// runner, which reported the job as cancelled after 45 minutes and named the assembly only in its
/// orphan-process cleanup, and locally, where a full run took 25 minutes instead of two. This assembly
/// is here because it waits on the same kind of thing in the same way, and neither run could say which
/// test did not return: a test with no deadline does not fail, it stops the run, and a stopped run
/// names nothing.
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
