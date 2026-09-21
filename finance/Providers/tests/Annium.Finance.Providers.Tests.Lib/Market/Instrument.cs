using Annium.Finance.Providers.Abstractions.Domain.Market;

namespace Annium.Finance.Providers.Tests.Lib.Market;

/// <summary>
/// A fake tradable instrument used to drive market/position/order test scenarios without a real exchange.
/// </summary>
/// <param name="Provider">The name of the (fake) provider the instrument belongs to.</param>
/// <param name="Symbol">The instrument's trading symbol (e.g. "BTCUSDT").</param>
/// <param name="LotSize">The quantity step orders must be a multiple of, in the instrument's base asset.</param>
/// <param name="TickSize">The price step order prices must be a multiple of.</param>
/// <param name="MinQty">The minimum order quantity, in the instrument's base asset.</param>
/// <param name="MaxQty">The maximum order quantity, in the instrument's base asset.</param>
/// <param name="MinPrice">The minimum allowed order price.</param>
/// <param name="MaxPrice">The maximum allowed order price.</param>
/// <param name="MinSum">The minimum order notional value (quantity multiplied by price).</param>
/// <param name="MaxSum">The maximum order notional value (quantity multiplied by price).</param>
/// <param name="MaxOrders">The maximum number of open orders allowed on the instrument.</param>
/// <param name="MinBuyPriceRatio">The lowest price a buy order may carry as a fraction of the reference price; zero, the default, leaves that end unbounded.</param>
/// <param name="MaxBuyPriceRatio">The highest price a buy order may carry as a fraction of the reference price; zero, the default, leaves that end unbounded.</param>
/// <param name="MinSellPriceRatio">The lowest price a sell order may carry as a fraction of the reference price; zero, the default, leaves that end unbounded.</param>
/// <param name="MaxSellPriceRatio">The highest price a sell order may carry as a fraction of the reference price; zero, the default, leaves that end unbounded.</param>
public sealed record Instrument(
    string Provider,
    string Symbol,
    decimal LotSize,
    decimal TickSize,
    decimal MinQty,
    decimal MaxQty,
    decimal MinPrice,
    decimal MaxPrice,
    decimal MinSum,
    decimal MaxSum,
    int MaxOrders,
    decimal MinBuyPriceRatio = decimal.Zero,
    decimal MaxBuyPriceRatio = decimal.Zero,
    decimal MinSellPriceRatio = decimal.Zero,
    decimal MaxSellPriceRatio = decimal.Zero
) : IInstrument
{
    /// <summary>Returns the instrument's symbol.</summary>
    /// <returns>The instrument's symbol.</returns>
    public override string ToString() => Symbol;
}
