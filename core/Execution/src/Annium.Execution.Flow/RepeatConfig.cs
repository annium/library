using System;

namespace Annium.Execution.Flow;

/// <summary>
/// How long <see cref="Repeat.UntilAsync"/> waits between attempts that achieve nothing.
/// </summary>
/// <param name="InitialDelay">The wait after the first fruitless attempt, and the wait it returns to once the work progresses again.</param>
/// <param name="MaxDelay">The longest the wait grows to.</param>
/// <param name="Factor">What the wait is multiplied by after each further fruitless attempt.</param>
public sealed record RepeatConfig(TimeSpan InitialDelay, TimeSpan MaxDelay, double Factor)
{
    /// <summary>
    /// The default for work that waits on a remote service: a fifth of a second at first, growing to a
    /// minute.
    /// </summary>
    /// <remarks>
    /// The ceiling matters more than the start. A service that answers a flood of requests by refusing them
    /// refuses for minutes, and a caller that keeps asking is what extends that - so the wait has to grow
    /// into the same order of magnitude rather than settling at a couple of seconds.
    /// </remarks>
    public static readonly RepeatConfig Upstream = new(TimeSpan.FromMilliseconds(200), TimeSpan.FromMinutes(1), 2);
}
