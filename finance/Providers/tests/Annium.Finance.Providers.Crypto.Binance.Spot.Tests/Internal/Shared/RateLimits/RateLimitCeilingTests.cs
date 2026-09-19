using System.Threading.Tasks;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.Shared.RateLimits;

/// <summary>
/// Pins the request-weight ceiling this venue is registered with, through the behaviour it produces.
/// </summary>
/// <remarks>
/// <para>
/// The ceiling (6000) and the water-mark fraction (0.8) are constants in two different files, and
/// neither is interesting alone — what decides whether a request goes out is the number they compose to,
/// 4800. So that is what this asserts: the limiter resolved from the real registration says yes one
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
    /// <summary>The water mark the registered ceiling composes to: floor(6000 * 0.8).</summary>
    private const int WaterMark = 4800;

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
        ctx.WithBinanceSpot();
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
    /// The registered decay lowers used weight by 300 every 3 seconds - the rate this venue's ceiling implies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 6000 a minute is 300 every three seconds, so here the pair is consistent. It is pinned all the same,
    /// and pinned now, because the two venues share these three numbers and differ on the first: the futures
    /// limiter was carrying this venue's 300 against a ceiling of 2400. A constant that is right by accident
    /// and a constant that is right on purpose look identical until one of them is copied.
    /// </para>
    /// <para>
    /// Bracketed from both sides. The first observation starts 250 above the mark and must clear in one tick,
    /// which fails for any step below 250; the second starts 350 above and must <em>not</em> clear in one
    /// tick, which fails for any step above 350. The waits are bounded to two intervals, so a decay ticking
    /// far slower than registered fails rather than passing inside a generous timeout.
    /// </para>
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = 60_000)]
    public async Task Decay_LowersTheRegisteredAmountOnTheRegisteredInterval()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var limiter = Get<IRateLimiter>();

        // act - 250 above the mark: one decay of 300 takes it under, one of 200 would not
        limiter.UsedWeight(WaterMark + 250);
        limiter.CanExecute().IsFalse($"allowed at {WaterMark + 250}, above the water mark");

        // assert
        await Expect.ToAsync(
            () => limiter.CanExecute().IsTrue("still refused after a decay that should have cleared it"),
            2 * DecayInterval
        );

        // act - 350 above the mark: one decay of 300 leaves it 50 above, so it stays refused
        limiter.UsedWeight(WaterMark + 350);

        // assert - a step of 350 or more would have cleared it here
        await Task.Delay(DecayInterval + DecayInterval / 2, ct);
        limiter.CanExecute().IsFalse("cleared after one decay, so the step is larger than the registered 300");

        // and the next decay does clear it
        await Expect.ToAsync(
            () => limiter.CanExecute().IsTrue("still refused after a second decay"),
            2 * DecayInterval
        );
    }

    /// <summary>The decay interval the venue registers, in milliseconds.</summary>
    private const int DecayInterval = 3_000;
}
