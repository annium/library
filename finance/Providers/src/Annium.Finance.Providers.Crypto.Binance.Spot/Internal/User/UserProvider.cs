using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Abstractions.Domain.User.Operations;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.User.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Logging;
using Annium.Net.Http;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User;

/// <summary>
/// Loads Binance spot account data: balances, open orders, order history and trade history.
/// </summary>
/// <remarks>
/// <para>
/// Every venue fact here comes from §10 of the provider manifest, collected from the stored
/// documentation snapshot on 2026-09-21. None of it is taken from the futures provider, which diverges
/// on the three that matter: the history window, the cost of an unscoped open-order list, and which end
/// of a trade page the cursor selects.
/// </para>
/// <para>
/// Spot has no positions. The account context therefore carries balances and an empty position set,
/// which is a statement about the venue rather than a gap - a spot account cannot hold one.
/// </para>
/// </remarks>
/// <param name="config">The resolved user configuration.</param>
/// <param name="signatureService">Signs outgoing requests.</param>
/// <param name="getAccountRequestFactory">Factory for requests against the account endpoint.</param>
/// <param name="getOrderRequestFactory">Factory for requests against the order endpoints.</param>
/// <param name="getTradeRequestFactory">Factory for requests against the trade endpoint.</param>
/// <param name="rateLimiter">Limits request weight against the exchange's rate limits.</param>
/// <param name="logger">The logger.</param>
internal class UserProvider(
    UserConfig config,
    ISignatureService signatureService,
    IHttpRequestFactory getAccountRequestFactory,
    IHttpRequestFactory getOrderRequestFactory,
    IHttpRequestFactory getTradeRequestFactory,
    IRateLimiter rateLimiter,
    ILogger logger
) : IUserProvider, ILogSubject
{
    /// <summary>
    /// The largest page either history endpoint accepts (`rest-api.md:4360`, `:4595`).
    /// </summary>
    private const int QueryLimit = 1000;

    /// <summary>
    /// The longest history window the venue accepts, on both history endpoints.
    /// </summary>
    /// <remarks>
    /// Twenty-four hours, and **not** the seven days the futures venue allows: "the time between
    /// `startTime` and `endTime` can't be longer than 24 hours" (`rest-api.md:4367`, `:4600`). A loop
    /// ported from the neighbour asks for a window this venue refuses, which is why the number is here
    /// with its citation rather than shared from anywhere.
    /// </remarks>
    private static long QueryWindow { get; } = (long)TimeSpan.FromHours(24).TotalMilliseconds;

    /// <summary>Gets the logger.</summary>
    public ILogger Logger { get; } = logger;

    /// <summary>
    /// Loads the account's balances, with an empty position set.
    /// </summary>
    /// <returns>A result carrying the account context, or a failure if it could not be loaded.</returns>
    public async Task<UserResult<UserContext?>> LoadContextAsync()
    {
        var result = await getAccountRequestFactory
            .New(config.HttpApi)
            .Get("/api/v3/account")
            .ReceiveWindow()
            .Sign(signatureService)
            .WithRateDelay1M(rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers)
            .AsUserResultAsync<IReadOnlyCollection<AssetModel>>();

        if (!result.IsSuccess)
        {
            if (result.IsFailure)
                this.Debug("failure: {result}", result);

            return UserResult.From(result, default(UserContext));
        }

        // a spot account holds no positions, so an empty set here is the venue's answer and not a gap
        return UserResult.Ok<UserContext?>(new UserContext(result.Data, Array.Empty<PositionModel>()));
    }

    /// <summary>
    /// Loads every open order, across all symbols.
    /// </summary>
    /// <remarks>
    /// Sent without a symbol, which the venue charges 80 for against 6 when scoped
    /// (`rest-api.md:4293`). The interface asks for every open order, so 80 is the price of the
    /// question rather than a choice - worth knowing when setting how often it is asked.
    /// </remarks>
    /// <returns>A result carrying the open orders, or a failure if they could not be loaded.</returns>
    public async Task<UserResult<IReadOnlyCollection<OrderModel>?>> LoadOpenOrdersAsync()
    {
        var result = await getOrderRequestFactory
            .New(config.HttpApi)
            .Get("/api/v3/openOrders")
            .ReceiveWindow()
            .Sign(signatureService)
            .WithRateDelay1M(rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers)
            .AsUserResultAsync<IReadOnlyCollection<OrderModel>>();

        if (!result.IsSuccess)
        {
            if (result.IsFailure)
                this.Debug("failure: {result}", result);

            return UserResult.From(result, default(IReadOnlyCollection<OrderModel>));
        }

        return UserResult.Ok<IReadOnlyCollection<OrderModel>?>(result.Data);
    }

    /// <summary>
    /// Loads a symbol's orders, either the most recent page or everything since a given moment.
    /// </summary>
    /// <param name="symbol">The instrument to load orders for; required by the venue.</param>
    /// <param name="since">The moment to load from, or null for the most recent page.</param>
    /// <returns>A result carrying the orders, or a failure if they could not be loaded.</returns>
    public async Task<UserResult<IReadOnlyCollection<OrderModel>?>> LoadOrdersAsync(string symbol, long? since)
    {
        if (since is null)
            return await LoadLatestAsync<OrderModel>(getOrderRequestFactory, "/api/v3/allOrders", symbol);

        return await LoadHistoryAsync<OrderModel>(getOrderRequestFactory, "/api/v3/allOrders", symbol, since.Value);
    }

    /// <summary>
    /// Loads a symbol's trades, either the most recent page or everything since a given moment.
    /// </summary>
    /// <param name="symbol">The instrument to load trades for; required by the venue.</param>
    /// <param name="since">The moment to load from, or null for the most recent page.</param>
    /// <returns>A result carrying the trades, or a failure if they could not be loaded.</returns>
    public async Task<UserResult<IReadOnlyCollection<TradeModel>?>> LoadTradesAsync(string symbol, long? since)
    {
        if (since is null)
            return await LoadLatestAsync<TradeModel>(getTradeRequestFactory, "/api/v3/myTrades", symbol);

        return await LoadHistoryAsync<TradeModel>(getTradeRequestFactory, "/api/v3/myTrades", symbol, since.Value);
    }

    /// <summary>
    /// Loads the most recent page a history endpoint will give for a symbol.
    /// </summary>
    /// <remarks>
    /// Both endpoints return the most recent rows when no cursor is sent (`rest-api.md:4364-4365`,
    /// `:4598-4599`), so asking with a page size and no window is what "latest" means here. That is the
    /// opposite of the futures trade endpoint, whose page cap selects the oldest rows - which is why
    /// this is stated rather than assumed.
    /// </remarks>
    /// <typeparam name="T">The model the endpoint answers with.</typeparam>
    /// <param name="factory">The request factory to send through.</param>
    /// <param name="path">The endpoint path.</param>
    /// <param name="symbol">The instrument to load for.</param>
    /// <returns>A result carrying the page, or a failure.</returns>
    private async Task<UserResult<IReadOnlyCollection<T>?>> LoadLatestAsync<T>(
        IHttpRequestFactory factory,
        string path,
        string symbol
    )
    {
        var result = await factory
            .New(config.HttpApi)
            .Get(path)
            .Param("symbol", symbol)
            .Param("limit", QueryLimit)
            .ReceiveWindow()
            .Sign(signatureService)
            .WithRateDelay1M(rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers)
            .AsUserResultAsync<IReadOnlyCollection<T>>();

        if (!result.IsSuccess)
        {
            if (result.IsFailure)
                this.Debug("failure: {result}", result);

            return UserResult.From(result, default(IReadOnlyCollection<T>));
        }

        return UserResult.Ok<IReadOnlyCollection<T>?>(result.Data);
    }

    /// <summary>
    /// Walks a history endpoint forward from a moment, one window at a time, until it runs out.
    /// </summary>
    /// <remarks>
    /// Windowed rather than cursored because the window is the constraint the venue states: 24 hours,
    /// refused beyond that. Each window asks for the maximum page; a window answering a full page is
    /// not evidence of more, since the page cap and the window cap are independent, so the walk is by
    /// time and the page size is only there to keep the number of requests down.
    /// </remarks>
    /// <typeparam name="T">The model the endpoint answers with.</typeparam>
    /// <param name="factory">The request factory to send through.</param>
    /// <param name="path">The endpoint path.</param>
    /// <param name="symbol">The instrument to load for.</param>
    /// <param name="since">The moment to load from.</param>
    /// <returns>A result carrying everything found, or the first failure.</returns>
    private async Task<UserResult<IReadOnlyCollection<T>?>> LoadHistoryAsync<T>(
        IHttpRequestFactory factory,
        string path,
        string symbol,
        long since
    )
    {
        var items = new List<T>();
        var start = since;
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        while (start < now)
        {
            var until = Math.Min(start + QueryWindow, now);

            var result = await factory
                .New(config.HttpApi)
                .Get(path)
                .Param("symbol", symbol)
                .Param("startTime", start)
                .Param("endTime", until)
                .Param("limit", QueryLimit)
                .ReceiveWindow()
                .Sign(signatureService)
                .WithRateDelay1M(rateLimiter)
                .WithLogFromWithHeaders(this, LogData.Headers)
                .AsUserResultAsync<IReadOnlyCollection<T>>();

            if (!result.IsSuccess)
            {
                if (result.IsFailure)
                    this.Debug("failure: {result}", result);

                return UserResult.From(result, default(IReadOnlyCollection<T>));
            }

            items.AddRange(result.Data);
            start = until;
        }

        return UserResult.Ok<IReadOnlyCollection<T>?>(items.ToArray());
    }
}
