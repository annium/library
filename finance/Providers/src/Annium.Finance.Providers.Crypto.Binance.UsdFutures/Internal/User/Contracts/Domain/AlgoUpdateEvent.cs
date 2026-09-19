using Annium.Finance.Providers.Abstractions.Domain.User;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;

/// <summary>
/// The user data stream <c>ALGO_UPDATE</c> event, raised when a conditional order is created, cancelled,
/// triggered or finished.
/// </summary>
/// <remarks>
/// Introduced with the conditional-order migration: an order placed on the algo endpoints is not announced
/// by <c>ORDER_TRADE_UPDATE</c> at all until its trigger fires and an ordinary order comes into being.
/// </remarks>
/// <param name="AlgoId">The exchange-assigned conditional order id.</param>
/// <param name="ClientAlgoId">The client-supplied id, which the ordinary order inherits once the trigger fires.</param>
/// <param name="Range">The position side the order acts on.</param>
/// <param name="Symbol">The instrument symbol.</param>
/// <param name="Type">The order type the conditional order will place.</param>
/// <param name="Side">The order side (buy/sell).</param>
/// <param name="TotalQty">The order's quantity.</param>
/// <param name="Price">The limit price the triggered order takes, or zero for the market-flavoured types.</param>
/// <param name="LevelPrice">The trigger price.</param>
/// <param name="ReduceOnly">Whether the order may only reduce an existing position.</param>
/// <param name="Status">The domain status this event's <c>algoStatus</c> maps to.</param>
/// <param name="IsSuperseded">Whether an ordinary order now accounts for this conditional order.</param>
/// <param name="UpdatedAt">The timestamp of this event, in Unix milliseconds.</param>
internal sealed record AlgoUpdateEvent(
    string AlgoId,
    string ClientAlgoId,
    OrientationRange Range,
    string Symbol,
    OrderType Type,
    OrderSide Side,
    decimal TotalQty,
    decimal Price,
    decimal LevelPrice,
    bool ReduceOnly,
    OrderStatus Status,
    bool IsSuperseded,
    long UpdatedAt
);
