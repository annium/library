namespace Annium.Finance.Providers.Tests.Lib;

/// <summary>
/// The trait every test class is sorted by, so a run can ask for the cheap tests, the ones that read from a
/// real exchange, or the ones that trade on it.
/// </summary>
/// <remarks>
/// This is the only thing separating a routine run from one that trades, which makes marking a test part
/// of writing it rather than a later tidy-up. There used to be a second mechanism - an environment
/// variable each exchange test was gated on - and it was dropped deliberately: it protected against a
/// trait being wrong, and a trait being wrong is an accepted risk here, while the cost was that every
/// live run needed a variable set from somewhere the recipe could not see.
///
/// Absence of the trait means <c>offline</c>. That is the safe default in the direction that matters: a new
/// test nobody marked joins the block that is always run, rather than the block that is never run.
/// </remarks>
public static class TestBlock
{
    /// <summary>The trait name every block is expressed with.</summary>
    public const string Name = "block";

    /// <summary>Connects to a real exchange and a real account, and mutates nothing.</summary>
    public const string Read = "read";

    /// <summary>Mutates the account: places orders, opens and closes positions.</summary>
    public const string Write = "write";

    /// <summary>
    /// How long a <see cref="Read"/> test may run before xUnit fails it, in milliseconds.
    /// </summary>
    /// <remarks>
    /// Every exchange-facing test needs one, because none of what they wait on ends by itself: an exchange
    /// unreachable from where the test runs answers nothing, and a wait with no deadline then lasts as long
    /// as whoever is watching. A nightly run met exactly that and sat for 42 minutes until the runner killed
    /// the job - which reports as <em>cancelled</em>, not failed, and so reads like somebody pressed a
    /// button rather than like a test that never returned.
    ///
    /// Two minutes is far longer than these take - a read is a handful of requests, each already bounded at
    /// 30 seconds by the HTTP layer - and far shorter than any runner's patience. The point is not to
    /// measure anything by it; it is that the run ends, names the test, and fails.
    /// </remarks>
    public const int ReadTimeoutMs = 120_000;

    /// <summary>
    /// How long a <see cref="Write"/> test may run before xUnit fails it, in milliseconds.
    /// </summary>
    /// <remarks>
    /// Longer than <see cref="ReadTimeoutMs"/> because these wait on an exchange to act rather than only to
    /// answer: an order has to reach the book, fill or be cancelled, and a position has to close. Still a
    /// deadline, and a generous one - a trading test that has not finished in five minutes is not being
    /// slow, it is stuck, and it is holding a real account open while it is.
    /// </remarks>
    public const int WriteTimeoutMs = 300_000;
}
