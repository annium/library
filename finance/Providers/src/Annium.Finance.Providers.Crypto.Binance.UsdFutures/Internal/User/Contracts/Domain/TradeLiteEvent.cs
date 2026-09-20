namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;

/// <summary>
/// The user data stream's <c>TRADE_LITE</c> event: the earliest notice of a fill the venue gives.
/// </summary>
/// <remarks>
/// <para>
/// It arrives <em>before</em> the order update that reports the same fill, which is the whole reason to
/// read it — and it is deliberately not turned into a trade. The payload carries no commission and no
/// realised PnL, so a trade built from it would be a trade with a zero fee: not missing data a caller can
/// see, but wrong data it cannot. What this event is good for is knowing sooner that there is a trade to
/// go and fetch.
/// </para>
/// <para>
/// Captured from the live stream on 2026-09-19; the venue documents the payload nowhere this module could
/// retrieve.
/// </para>
/// </remarks>
/// <param name="Symbol">The instrument the fill happened on.</param>
/// <param name="OrderId">The exchange-assigned id of the order that filled.</param>
/// <param name="ClientOrderId">The client-supplied id of that order.</param>
/// <param name="TradeId">The exchange-assigned id of the fill itself.</param>
/// <param name="Price">The price the fill happened at.</param>
/// <param name="Qty">The quantity this fill covered.</param>
/// <param name="IsMaker">Whether the fill was on the maker side.</param>
/// <param name="At">The event's timestamp, in Unix milliseconds.</param>
internal sealed record TradeLiteEvent(
    string Symbol,
    string OrderId,
    string ClientOrderId,
    string TradeId,
    decimal Price,
    decimal Qty,
    bool IsMaker,
    long At
);
