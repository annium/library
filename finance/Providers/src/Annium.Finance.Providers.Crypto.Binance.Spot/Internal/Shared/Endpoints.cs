using System;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Internal.Shared;

/// <summary>
/// Base URLs and fixed paths of the Binance spot REST and websocket APIs.
/// </summary>
internal static class Endpoints
{
    /// <summary>Gets the base URL of the spot REST API.</summary>
    public static Uri HttpApi { get; } = new("https://api.binance.com");

    /// <summary>Gets the base URL of the spot market data websocket stream.</summary>
    public static Uri WsApi { get; } = new("wss://stream.binance.com");

    /// <summary>The relative path appended to <see cref="WsApi"/> when opening a market data connection.</summary>
    public const string MarketWsUriPath = "/stream";

    /// <summary>Gets the URL of the spot WebSocket API, which carries the user data stream.</summary>
    /// <remarks>
    /// <para>
    /// A different host from <see cref="WsApi"/>, and a different protocol: this one is request/response,
    /// where a subscription is a method call and the account's events come back on the same connection.
    /// Documented at `web-socket-api.md:102`, with the route in the URL rather than in a path appended to
    /// it — the whole value is the endpoint.
    /// </para>
    /// <para>
    /// It replaces the listen-key mechanism, which this venue deprecated and then removed from its
    /// reference entirely: neither `listenKey` nor `userDataStream` appears anywhere in `rest-api.md`.
    /// See §11 of the provider manifest for the census and for what the two remaining sources disagree
    /// about.
    /// </para>
    /// </remarks>
    public static Uri UserWsApi { get; } = new("wss://ws-api.binance.com:443/ws-api/v3");

    /// <summary>The relative path of the place-order endpoint, appended to <see cref="HttpApi"/>.</summary>
    public const string InitOrderUriPath = "/api/v3/order";

    /// <summary>The relative path of the cancel-and-replace endpoint, appended to <see cref="HttpApi"/>.</summary>
    /// <remarks>
    /// What this venue offers in place of an order amendment. Its own amend endpoint reduces quantity and
    /// nothing else, so a change of price is a cancel and a new order here — at the back of the queue, and
    /// against the unfilled-order budget. `rest-api.md:2578`, and §11 of the manifest.
    /// </remarks>
    public const string ModifyOrderUriPath = "/api/v3/order/cancelReplace";

    /// <summary>The relative path of the cancel-order endpoint, appended to <see cref="HttpApi"/>.</summary>
    public const string CancelOrderUriPath = "/api/v3/order";

    /// <summary>The relative path of the cancel-all-open-orders endpoint, appended to <see cref="HttpApi"/>.</summary>
    /// <remarks>A symbol is required here; this venue has no unscoped cancel-all (`rest-api.md:2453`).</remarks>
    public const string CancelAllOrdersUriPath = "/api/v3/openOrders";

    /// <summary>
    /// The server time path, appended to <see cref="HttpApi"/>.
    /// </summary>
    /// <remarks>
    /// <c>v3</c>, like the rest of the spot surface. It was <c>v1</c> here for as long as nobody ran the
    /// thing: the contract manifest recorded the oddity as a divergence and never checked it against the
    /// documented endpoint list, so a curiosity stood in for a verified fact until a live run failed on it.
    /// </remarks>
    public const string ServerTimeUriPath = "/api/v3/time";
}
