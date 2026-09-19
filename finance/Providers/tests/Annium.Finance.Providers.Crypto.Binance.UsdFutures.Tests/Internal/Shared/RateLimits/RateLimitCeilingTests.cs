using System.Threading.Tasks;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.Shared.RateLimits;

/// <summary>
/// Pins the request-weight ceiling this venue is registered with, through the behaviour it produces.
/// </summary>
/// <remarks>
/// <para>
/// The ceiling (2400) and the water-mark fraction (0.8) are constants in two different files, and
/// neither is interesting alone — what decides whether a request goes out is the number they compose to,
/// 1920. So that is what this asserts: the limiter resolved from the real registration says yes one
/// weight below it and no at it.
/// </para>
/// <para>
/// Both numbers had been `none`. They are the shape most likely to be copied wrong and least likely to be
/// noticed: too high and the account earns a ban this class exists to prevent, too low and throughput is
/// quietly throttled with nothing to point at. Neither shows up as a failing test — the first shows up as
/// an IP ban, the second as everything being slow.
/// </para>
/// </remarks>
public class RateLimitCeilingTests : ProvidersTestBase
{
    /// <summary>The water mark the registered ceiling composes to: floor(2400 * 0.8).</summary>
    private const int WaterMark = 1920;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitCeilingTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public RateLimitCeilingTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the provider, so the limiter under test is the one production resolves.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceUsdFutures();
    }

    /// <summary>
    /// One weight below the water mark a request is allowed; at it, it is not.
    /// </summary>
    [Fact]
    public void WaterMark_IsTheRegisteredCeilingTimesTheFraction()
    {
        // arrange
        var limiter = Get<IRateLimiter>();

        // act & assert - just under
        limiter.UsedWeight(WaterMark - 1);
        limiter.CanExecute().IsTrue($"refused at {WaterMark - 1}, one weight below the water mark");

        // act & assert - at the mark
        limiter.UsedWeight(WaterMark);
        limiter.CanExecute().IsFalse($"allowed at {WaterMark}, which is the water mark itself");
    }

    /// <summary>
    /// The registered decay lowers used weight by 120 every 3 seconds - the rate the documented ceiling
    /// implies, and not the neighbouring venue's.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 2400 a minute is 120 every three seconds. This venue registered 300, which is spot's figure because
    /// spot allows 6000 a minute, so the limiter returned budget two and a half times faster than the
    /// exchange did. A rate too small leaves the connector throttled long after the exchange has forgiven it;
    /// too large and it resumes while the exchange still counts the weight - which is how an account earns a
    /// ban from code that believes it is being careful.
    /// </para>
    /// <para>
    /// Bracketed from both sides, because the previous version of this test could only tell the step from a
    /// larger one and passed against 300 by construction. The first observation starts 100 above the mark and
    /// must clear in one tick, which fails for any step below 100; the second starts 150 above and must
    /// <em>not</em> clear in one tick, which fails for any step above 150. Together they admit only a step
    /// between 100 and 150.
    /// </para>
    /// <para>
    /// The interval is bracketed too, by bounding the waits rather than allowing twenty seconds for a three
    /// second tick: an interval much longer than the registered one no longer passes unnoticed.
    /// </para>
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = 60_000)]
    public async Task Decay_LowersTheRegisteredAmountOnTheRegisteredInterval()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var limiter = Get<IRateLimiter>();

        // act - 100 above the mark: one decay of 120 takes it under, one of 80 would not
        limiter.UsedWeight(WaterMark + 100);
        limiter.CanExecute().IsFalse($"allowed at {WaterMark + 100}, above the water mark");

        // assert - bounded to two intervals, so a decay that ticks far slower fails here
        await Expect.ToAsync(
            () => limiter.CanExecute().IsTrue("still refused after a decay that should have cleared it"),
            2 * DecayInterval
        );

        // act - 150 above the mark: one decay of 120 leaves it 30 above, so it stays refused
        limiter.UsedWeight(WaterMark + 150);

        // assert - a step of 150 or more would have cleared it here, and this is what bounds it from above
        await Task.Delay(DecayInterval + DecayInterval / 2, ct);
        limiter.CanExecute().IsFalse("cleared after one decay, so the step is larger than the registered 120");

        // and the next decay does clear it
        await Expect.ToAsync(
            () => limiter.CanExecute().IsTrue("still refused after a second decay"),
            2 * DecayInterval
        );
    }

    /// <summary>The decay interval the venue registers, in milliseconds.</summary>
    private const int DecayInterval = 3_000;
}
