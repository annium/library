namespace Annium.Execution.Flow;

/// <summary>
/// What one attempt at a repeated piece of work achieved.
/// </summary>
public enum AttemptOutcome
{
    /// <summary>The attempt moved the work forward; the next one is worth making at once.</summary>
    Progressed,

    /// <summary>The attempt gained nothing, but repeating it later may still work.</summary>
    Stalled,

    /// <summary>The attempt failed in a way repeating will not fix.</summary>
    Failed,
}

/// <summary>
/// The result of one attempt, as <see cref="Repeat.UntilAsync"/> reads it.
/// </summary>
/// <param name="Outcome">What the attempt achieved.</param>
/// <param name="Message">Why, for a stalled or failed attempt; empty otherwise.</param>
public readonly record struct Attempt(AttemptOutcome Outcome, string Message = "")
{
    /// <summary>An attempt that moved the work forward.</summary>
    public static readonly Attempt Progressed = new(AttemptOutcome.Progressed);

    /// <summary>
    /// Builds the result of an attempt that gained nothing but is worth repeating.
    /// </summary>
    /// <param name="message">Why it gained nothing.</param>
    /// <returns>The attempt result.</returns>
    public static Attempt Stalled(string message) => new(AttemptOutcome.Stalled, message);

    /// <summary>
    /// Builds the result of an attempt that repeating will not fix.
    /// </summary>
    /// <param name="message">Why it failed.</param>
    /// <returns>The attempt result.</returns>
    public static Attempt Failed(string message) => new(AttemptOutcome.Failed, message);
}
