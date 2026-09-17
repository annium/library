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
}
