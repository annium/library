namespace Annium.Finance.Providers.Abstractions.Domain.Market;

/// <summary>
/// Represents the trading constraints of an instrument: the quantity and price steps and bounds orders must satisfy.
/// </summary>
/// <remarks>
/// The price ratios express a band around a <b>reference price the provider itself computes</b> - an average
/// of recent trades over a window, a mark price, or whatever else it publishes - and not around the top of
/// the book. A caller substituting the current bid or ask is approximating, and an order priced near the edge
/// of the band may still be refused. The band is also per side and need not be symmetric: a provider may cap
/// how high a buy may be priced while leaving it free to sit arbitrarily far below the market.
/// </remarks>
public interface IInstrument
{
    /// <summary>Gets the quantity step orders must be a multiple of, in the instrument's base asset.</summary>
    decimal LotSize { get; }

    /// <summary>Gets the price step order prices must be a multiple of.</summary>
    decimal TickSize { get; }

    /// <summary>Gets the minimum order quantity, in the instrument's base asset.</summary>
    decimal MinQty { get; }

    /// <summary>Gets the maximum order quantity, in the instrument's base asset.</summary>
    decimal MaxQty { get; }

    /// <summary>Gets the minimum allowed order price, or zero where the provider does not bound it.</summary>
    decimal MinPrice { get; }

    /// <summary>Gets the maximum allowed order price, or zero where the provider does not bound it.</summary>
    decimal MaxPrice { get; }

    /// <summary>Gets the lowest price a buy order may carry as a fraction of the provider's reference price, or zero where the provider does not bound it.</summary>
    decimal MinBuyPriceRatio { get; }

    /// <summary>Gets the highest price a buy order may carry as a fraction of the provider's reference price, or zero where the provider does not bound it.</summary>
    decimal MaxBuyPriceRatio { get; }

    /// <summary>Gets the lowest price a sell order may carry as a fraction of the provider's reference price, or zero where the provider does not bound it.</summary>
    decimal MinSellPriceRatio { get; }

    /// <summary>Gets the highest price a sell order may carry as a fraction of the provider's reference price, or zero where the provider does not bound it.</summary>
    decimal MaxSellPriceRatio { get; }

    /// <summary>Gets the minimum order notional value (quantity multiplied by price).</summary>
    decimal MinSum { get; }

    /// <summary>Gets the maximum order notional value (quantity multiplied by price).</summary>
    decimal MaxSum { get; }
}
