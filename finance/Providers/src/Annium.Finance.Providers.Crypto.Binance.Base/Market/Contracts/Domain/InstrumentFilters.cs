namespace Annium.Finance.Providers.Crypto.Binance.Base.Market.Contracts.Domain;

/// <summary>The set of Binance instrument trading filters (<c>LOT_SIZE</c>, <c>PRICE_FILTER</c>, notional and max-orders) that constrain orders placed on a symbol.</summary>
/// <param name="LotSizeFilter">The quantity constraints (<c>LOT_SIZE</c>).</param>
/// <param name="PriceFilter">The price constraints (<c>PRICE_FILTER</c>).</param>
/// <param name="NotionalFilter">The notional value constraints.</param>
/// <param name="PercentPriceFilter">The band an order price must sit in relative to the exchange's reference price.</param>
/// <param name="MaxOrdersFilter">The maximum number of open orders allowed for the symbol.</param>
public sealed record InstrumentFilters(
    LotSizeFilter LotSizeFilter,
    PriceFilter PriceFilter,
    NotionalFilter NotionalFilter,
    PercentPriceFilter PercentPriceFilter,
    MaxOrdersFilter MaxOrdersFilter
);

/// <summary>Binance's <c>LOT_SIZE</c> filter, constraining the order quantity for a symbol.</summary>
/// <param name="MinQty">The minimum order quantity.</param>
/// <param name="MaxQty">The maximum order quantity.</param>
/// <param name="StepSize">The increment the order quantity must be a multiple of.</param>
public sealed record LotSizeFilter(decimal MinQty, decimal MaxQty, decimal StepSize);

/// <summary>Binance's <c>PRICE_FILTER</c>, constraining the order price for a symbol.</summary>
/// <param name="MinPrice">The minimum order price.</param>
/// <param name="MaxPrice">The maximum order price.</param>
/// <param name="TickSize">The increment the order price must be a multiple of.</param>
public sealed record PriceFilter(decimal MinPrice, decimal MaxPrice, decimal TickSize);

/// <summary>Binance's notional value filter, constraining the minimum and maximum order value for a symbol.</summary>
/// <param name="MinNotional">The minimum order notional value (price times quantity).</param>
/// <param name="MaxNotional">The maximum order notional value (price times quantity).</param>
public sealed record NotionalFilter(decimal MinNotional, decimal MaxNotional);

/// <summary>
/// The band a limit order's price must sit in, as fractions of the exchange's own reference price, with zero
/// meaning that end of the band is not bounded for that side.
/// </summary>
/// <remarks>
/// Written in the model's vocabulary rather than the wire's, because the two market types spell this filter
/// differently: one gives four multipliers, one per side and end, against an average of recent trades; the
/// other gives a single pair against the mark price, and applies only the upper one to buys and only the
/// lower one to sells. Mapping both onto per-side ends keeps the asymmetry instead of averaging it away -
/// reading the futures pair as a two-sided band would refuse resting orders the exchange accepts.
/// </remarks>
/// <param name="MinBuyRatio">The lowest price a buy order may carry, as a fraction of the reference price.</param>
/// <param name="MaxBuyRatio">The highest price a buy order may carry, as a fraction of the reference price.</param>
/// <param name="MinSellRatio">The lowest price a sell order may carry, as a fraction of the reference price.</param>
/// <param name="MaxSellRatio">The highest price a sell order may carry, as a fraction of the reference price.</param>
public sealed record PercentPriceFilter(
    decimal MinBuyRatio,
    decimal MaxBuyRatio,
    decimal MinSellRatio,
    decimal MaxSellRatio
);

/// <summary>Binance's filter on the maximum number of open orders allowed for a symbol.</summary>
/// <param name="MaxOrders">The maximum number of open orders allowed.</param>
public sealed record MaxOrdersFilter(int MaxOrders);
