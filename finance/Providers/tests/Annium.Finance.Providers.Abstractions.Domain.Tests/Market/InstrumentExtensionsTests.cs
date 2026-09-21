using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Tests.Lib.Market;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Abstractions.Domain.Tests.Market;

/// <summary>
/// Pins the rounding, clamping and validity rules <see cref="InstrumentExtensions"/> derives from an
/// <see cref="IInstrument"/>'s lot size, tick size and quantity/notional bounds.
/// </summary>
public class InstrumentExtensionsTests
{
    /// <summary>An instrument with default tick and lot sizes, used by every test that doesn't need bespoke bounds.</summary>
    private readonly IInstrument _instrument = InstrumentHelper.DefaultInstrument;

    /// <summary>
    /// Pins the limits the fake instrument derives from its lot and tick size. Every boundary case in this
    /// file is written against those limits by name — <c>ToValidQty(0.05m).Is(_instrument.MinQty)</c> and the
    /// like — so a change to how they are derived would move every one of those boundaries and keep them all
    /// agreeing with each other. Stating the numbers once is what stops the rest of the file from being
    /// self-referential.
    /// </summary>
    [Fact]
    public void DefaultInstrument_HasTheLimitsEverythingElseIsMeasuredAgainst()
    {
        // assert - lot 0.1 and tick 0.01
        _instrument.LotSize.Is(0.1m);
        _instrument.TickSize.Is(0.01m);
        _instrument.MinQty.Is(1m);
        _instrument.MaxQty.Is(10m);
        _instrument.MinPrice.Is(0.01m);
        _instrument.MaxPrice.Is(10_000m);
        _instrument.MinSum.Is(1m);
    }

    /// <summary>
    /// Verifies that <see cref="InstrumentExtensions.TickPrecision{TInstrument}"/> counts the decimal digits of the
    /// tick size, independent of the price's own precision.
    /// </summary>
    [Fact]
    public void TickPrecision()
    {
        // assert
        InstrumentHelper.CreateInstrument("x", "y", 100m, 10m).TickPrecision().Is(0);
        InstrumentHelper.CreateInstrument("x", "y", 75m, 7.50m).TickPrecision().Is(1);
        InstrumentHelper.CreateInstrument("x", "y", 7.5m, 0.75m).TickPrecision().Is(2);
        InstrumentHelper.CreateInstrument("x", "y", 1m, 0.01m).TickPrecision().Is(2);
    }

    /// <summary>
    /// Verifies that <see cref="InstrumentExtensions.ToValidQty{TInstrument}"/> rounds a quantity to the lot size
    /// and then clamps it to the instrument's minimum and maximum quantity.
    /// </summary>
    [Fact]
    public void ToValidQty()
    {
        // assert
        _instrument.ToValidQty(1.56m).Is(1.5m);
        _instrument.ToValidQty(0.05m).Is(_instrument.MinQty);
        _instrument.ToValidQty(105m).Is(_instrument.MaxQty);
    }

    /// <summary>
    /// Verifies that <see cref="InstrumentExtensions.IsValidQtyPrice{TInstrument}"/> rejects a quantity or price
    /// that isn't already aligned, and a notional value under the minimum sum, while accepting values that
    /// satisfy every constraint at once.
    /// </summary>
    [Fact]
    public void IsValidQtyPrice()
    {
        // assert
        // invalid qty
        _instrument.IsValidQtyPrice(2.01m, 0.1m).IsFalse();
        // invalid price
        _instrument.IsValidQtyPrice(2m, 0.101m).IsFalse();
        // less than min sum
        _instrument.IsValidQtyPrice(2m, 0.1m).IsFalse();
        // valid
        _instrument.IsValidQtyPrice(2m, 0.5m).IsTrue();
    }

    /// <summary>Verifies that <see cref="InstrumentExtensions.ToTickSizeDown{TInstrument}"/> always rounds a price down to the nearest tick.</summary>
    [Fact]
    public void ToTickSizeDown()
    {
        // assert
        _instrument.ToTickSizeDown(0.126m).Is(0.12m);
    }

    /// <summary>Verifies that <see cref="InstrumentExtensions.ToTickSizeRound{TInstrument}"/> rounds a price to the nearest tick, up or down depending on which is closer.</summary>
    [Fact]
    public void ToTickSizeRound()
    {
        // arrange
        _instrument.ToTickSizeRound(0.124m).Is(0.12m);
        _instrument.ToTickSizeRound(0.126m).Is(0.13m);
    }

    /// <summary>Verifies that <see cref="InstrumentExtensions.ToTickSizeUp{TInstrument}"/> always rounds a price up to the nearest tick.</summary>
    [Fact]
    public void ToTickSizeUp()
    {
        // arrange
        _instrument.ToTickSizeUp(0.124m).Is(0.13m);
    }

    /// <summary>Verifies that <see cref="InstrumentExtensions.ToLotSize{TInstrument}"/> rounds a quantity down to the nearest lot size.</summary>
    [Fact]
    public void ToLotSize()
    {
        // arrange
        _instrument.ToLotSize(0.12m).Is(0.1m);
    }

    /// <summary>
    /// Verifies that a negative quantity - the way a sell or short size is expressed - is rounded towards
    /// zero rather than down, so aligning it can only shrink the order, never grow it.
    /// </summary>
    [Fact]
    public void ToLotSize_NegativeQty_RoundsTowardsZero()
    {
        // assert
        _instrument.ToLotSize(-0.12m).Is(-0.1m, "aligning a sell size must not ask for more than was requested");
        _instrument.ToLotSize(-1.56m).Is(-1.5m);
        _instrument.ToValidQty(-105m).Is(-_instrument.MaxQty);
        // a sell smaller than one lot aligns to zero; it must still come back a sell. Reading the side off
        // the aligned value instead of the request turns it into a buy of the minimum quantity
        _instrument.ToLotSize(-0.05m).Is(0m);
        _instrument.ToValidQty(-0.05m).Is(-_instrument.MinQty, "a sell must never come back as a buy");
    }

    /// <summary>
    /// Verifies that an instrument reporting no lot or tick step leaves quantities and prices untouched
    /// instead of dividing by zero.
    /// </summary>
    [Fact]
    public void ZeroLotAndTick_LeaveValuesUntouched()
    {
        // arrange
        var instrument = InstrumentHelper.CreateInstrument("x", "y", 0m, 0m);

        // assert
        instrument.ToLotSize(0.123m).Is(0.123m);
        instrument.ToTickSizeDown(0.123m).Is(0.123m);
        instrument.ToTickSizeRound(0.123m).Is(0.123m);
        instrument.ToTickSizeUp(0.123m).Is(0.123m);
    }

    /// <summary>
    /// Verifies that a price outside the instrument's own price bounds is rejected even when it is tick
    /// aligned and its notional value is in range - the constraint the exchange reports and would enforce.
    /// </summary>
    [Fact]
    public void IsValidQtyPrice_PriceOutOfBounds_IsRejected()
    {
        // arrange - prices allowed in [10, 100], every value below tick aligned and comfortably inside
        // the quantity and notional bounds, so the price bound is the only thing any case turns on
        var instrument = new Instrument("fake", "XY", 1m, 1m, 1m, 100m, 10m, 100m, 1m, decimal.MaxValue, int.MaxValue);

        // assert
        instrument.IsValidQtyPrice(1m, 9m).IsFalse("a price below the instrument's minimum is not valid");
        instrument.IsValidQtyPrice(1m, 101m).IsFalse("a price above the instrument's maximum is not valid");
        instrument.IsValidQtyPrice(1m, 10m).IsTrue("the minimum price itself is valid");
        instrument.IsValidQtyPrice(1m, 100m).IsTrue("the maximum price itself is valid");
    }

    /// <summary>
    /// Verifies that a bound the provider reports as zero is not enforced. Binance's price filter uses zero
    /// to say it does not bound that side, exactly as its lot and tick sizes do, so reading it as a literal
    /// limit would reject every price on such a symbol.
    /// </summary>
    [Fact]
    public void IsValidQtyPrice_UnboundedPrice_IsNotEnforced()
    {
        // arrange - neither bound is set, as an exchange reports for a symbol it does not limit
        var instrument = new Instrument("fake", "XY", 1m, 1m, 1m, 100m, 0m, 0m, 1m, decimal.MaxValue, int.MaxValue);

        // assert
        instrument.IsValidQtyPrice(1m, 1m).IsTrue("an unset minimum must not reject a low price");
        instrument.IsValidQtyPrice(1m, 1_000_000m).IsTrue("an unset maximum must not reject a high price");
    }

    /// <summary>
    /// Verifies that each end of the price band is read for the side it was published for.
    /// </summary>
    /// <remarks>
    /// Four different ratios, so that a band read with the sides transposed - or with one side's end mirrored
    /// onto the other - fails here rather than in a live order the exchange refuses by naming a filter.
    /// </remarks>
    [Fact]
    public void SidePrice_IsTheBandEndForThatSide()
    {
        // arrange - buy in [0.5, 1.2] of the reference, sell in [0.8, 2]
        var instrument = Banded(0.5m, 1.2m, 0.8m, 2m);

        // assert - against a reference price of 100
        instrument.MinSidePrice(OrderSide.Buy, 100m).Is(50m);
        instrument.MaxSidePrice(OrderSide.Buy, 100m).Is(120m);
        instrument.MinSidePrice(OrderSide.Sell, 100m).Is(80m);
        instrument.MaxSidePrice(OrderSide.Sell, 100m).Is(200m);
    }

    /// <summary>
    /// Verifies that a price is checked against the band of the side it would be placed on, and that the ends
    /// themselves are allowed.
    /// </summary>
    [Fact]
    public void IsValidSidePrice_ChecksTheSidesBand()
    {
        // arrange - buy in [0.5, 1.2] of the reference, sell in [0.8, 2]
        var instrument = Banded(0.5m, 1.2m, 0.8m, 2m);

        // assert - a price valid for one side need not be valid for the other, which is the whole point of
        // a per-side band: 60 rests inside a buy's range and below a sell's floor
        instrument.IsValidSidePrice(60m, OrderSide.Buy, 100m).IsTrue("60 is inside the buy band");
        instrument.IsValidSidePrice(60m, OrderSide.Sell, 100m).IsFalse("60 is below the sell floor");
        instrument.IsValidSidePrice(150m, OrderSide.Buy, 100m).IsFalse("150 is above the buy ceiling");
        instrument.IsValidSidePrice(150m, OrderSide.Sell, 100m).IsTrue("150 is inside the sell band");

        // and the ends are inclusive, as the exchange states them
        instrument.IsValidSidePrice(50m, OrderSide.Buy, 100m).IsTrue("the buy floor itself is valid");
        instrument.IsValidSidePrice(120m, OrderSide.Buy, 100m).IsTrue("the buy ceiling itself is valid");
    }

    /// <summary>
    /// Verifies that an end reported as zero is not enforced, and that the two ends are read independently.
    /// </summary>
    /// <remarks>
    /// This is the shape one of the two Binance market types publishes: a buy is capped from above and given
    /// no floor at all. Reading the missing floor as a literal zero-and-binding bound would refuse every
    /// resting order priced away from the market - orders that market type accepts, and that our own trading
    /// block places on every run.
    /// </remarks>
    [Fact]
    public void IsValidSidePrice_UnboundedEnd_IsNotEnforced()
    {
        // arrange - a buy capped at 1.15 with no floor, a sell floored at 0.85 with no ceiling
        var instrument = Banded(decimal.Zero, 1.15m, 0.85m, decimal.Zero);

        // assert
        instrument.IsValidSidePrice(1m, OrderSide.Buy, 100m).IsTrue("an unset buy floor must not reject");
        instrument.IsValidSidePrice(116m, OrderSide.Buy, 100m).IsFalse("the buy ceiling still binds");
        instrument.IsValidSidePrice(1_000_000m, OrderSide.Sell, 100m).IsTrue("an unset sell ceiling must not reject");
        instrument.IsValidSidePrice(84m, OrderSide.Sell, 100m).IsFalse("the sell floor still binds");
    }

    /// <summary>
    /// Verifies that an instrument with no band at all accepts any price on either side.
    /// </summary>
    [Fact]
    public void IsValidSidePrice_NoBand_AcceptsAnything()
    {
        // arrange - what a symbol published without the filter reads as
        var instrument = Banded(decimal.Zero, decimal.Zero, decimal.Zero, decimal.Zero);

        // assert
        instrument.IsValidSidePrice(1m, OrderSide.Buy, 100m).IsTrue("no band means no buy bound");
        instrument.IsValidSidePrice(1_000_000m, OrderSide.Sell, 100m).IsTrue("no band means no sell bound");
    }

    /// <summary>
    /// Builds an instrument whose only interesting limits are its price band.
    /// </summary>
    /// <param name="minBuy">The lowest price a buy may carry, as a fraction of the reference price.</param>
    /// <param name="maxBuy">The highest price a buy may carry, as a fraction of the reference price.</param>
    /// <param name="minSell">The lowest price a sell may carry, as a fraction of the reference price.</param>
    /// <param name="maxSell">The highest price a sell may carry, as a fraction of the reference price.</param>
    /// <returns>The instrument.</returns>
    private static Instrument Banded(decimal minBuy, decimal maxBuy, decimal minSell, decimal maxSell) =>
        new(
            "fake",
            "XY",
            1m,
            1m,
            1m,
            100m,
            0m,
            0m,
            1m,
            decimal.MaxValue,
            int.MaxValue,
            minBuy,
            maxBuy,
            minSell,
            maxSell
        );
}
