using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Abstractions.Domain.User.Operations;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Core.User;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.User.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;
using Annium.Logging;
using Annium.Net.Http;
using NodaTime;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User;

/// <summary>
/// Loads USD-M futures account state, orders and trades straight from the Binance REST API, signing every
/// request with the account's API secret.
/// </summary>
/// <param name="config">The resolved user connector configuration.</param>
/// <param name="timeProvider">Supplies the current time, used to bound history queries.</param>
/// <param name="signatureService">Signs outgoing REST requests.</param>
/// <param name="getAccountRequestFactory">Factory for requests against the account info endpoint.</param>
/// <param name="getOrderRequestFactory">Factory for requests against the order lookup endpoints.</param>
/// <param name="getTradeRequestFactory">Factory for requests against the trade lookup endpoint.</param>
/// <param name="algoOrderRequestFactory">Factory for requests against the conditional ("algo") order endpoints.</param>
/// <param name="rateLimiter">Limits request weight against the exchange's rate limits.</param>
/// <param name="logger">The logger.</param>
internal class UserProvider(
    UserConfig config,
    ITimeProvider timeProvider,
    ISignatureService signatureService,
    IHttpRequestFactory getAccountRequestFactory,
    IHttpRequestFactory getOrderRequestFactory,
    IHttpRequestFactory getTradeRequestFactory,
    IHttpRequestFactory algoOrderRequestFactory,
    IRateLimiter rateLimiter,
    ILogger logger
) : IUserProvider, ILogSubject
{
    /// <summary>The maximum number of orders returned by a single order history request.</summary>
    private const int OrderQueryLimit = 1000;

    /// <summary>How far back the latest-trades read looks, in milliseconds.</summary>
    /// <remarks>
    /// One hour. See <c>LoadLatestTradesAsync</c> for why this endpoint is bounded by time rather than by
    /// page size - a limit on it selects the oldest trades, not the newest.
    /// </remarks>
    private const long LatestTradesWindow = 60L * 60 * 1000;

    /// <summary>The maximum number of trades returned by a single trade history request.</summary>
    private const int TradeQueryLimit = 1000;

    /// <summary>The maximum time span covered by a single order history request (7 days).</summary>
    private static long OrderQueryWindow { get; } = TimeSpan.FromDays(7).TotalMilliseconds.FloorInt64();

    /// <summary>The maximum time span covered by a single trade history request (7 days).</summary>
    private static long TradeQueryWindow { get; } = TimeSpan.FromDays(7).TotalMilliseconds.FloorInt64();

    /// <summary>Gets the logger for this provider.</summary>
    public ILogger Logger { get; } = logger;

    /// <summary>
    /// Loads the account's asset balances and positions and maps them into the library's generic account
    /// context. An asset's usable balance is the free balance, and its locked balance is the sum of initial and
    /// maintenance margin.
    /// </summary>
    /// <remarks>
    /// A failure is returned, not reported: every method here hands its result to a caller - a loader, a
    /// connector - whose job is to decide what it means and say so once. Logging it as an error here as well
    /// meant the same refusal was written twice, by the half of the pair that had decided nothing.
    /// </remarks>
    /// <returns>A result carrying the resolved account context, or a failure status if it could not be loaded.</returns>
    public async Task<UserResult<UserContext?>> LoadContextAsync()
    {
        var result = await getAccountRequestFactory
            .New(config.HttpApi)
            .Get("/fapi/v2/account")
            .ReceiveWindow()
            .Sign(signatureService)
            .WithRateDelay1M(rateLimiter)
            // .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
            .WithLogFromWithHeaders(this, LogData.Headers)
            .AsUserResultAsync<AccountResponse>();

        if (!result.IsSuccess)
        {
            if (result.IsFailure)
                this.Debug("failure: {result}", result);

            return UserResult.From(result, default(UserContext));
        }

        var assets = result
            .Data.Balances.Select(x => new AssetModel(x.Asset, x.Free, x.InitialMargin + x.MaintenanceMargin))
            .ToArray();

        var positions = result
            .Data.Positions.Select(x => new PositionModel(x.Symbol, x.Orientation, x.MarginType, x.Leverage, x.Amount))
            .ToArray();

        return UserResult.Ok<UserContext?>(new UserContext(assets, positions));
    }

    /// <summary>
    /// Loads all currently open orders, across all symbols.
    /// </summary>
    /// <returns>A result carrying the open orders, or a failure status if they could not be loaded.</returns>
    public async Task<UserResult<IReadOnlyCollection<OrderModel>?>> LoadOpenOrdersAsync()
    {
        var result = await getOrderRequestFactory
            .New(config.HttpApi)
            .Get("/fapi/v1/openOrders")
            .ReceiveWindow()
            .Sign(signatureService)
            .WithRateDelay1M(rateLimiter)
            // .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
            .WithLogFromWithHeaders(this, LogData.Headers)
            .AsUserResultAsync<IReadOnlyCollection<OrderModel>>();

        if (!result.IsSuccess)
        {
            if (result.IsFailure)
                this.Debug("failure: {result}", result);

            return UserResult.From(result, default(IReadOnlyCollection<OrderModel>));
        }

        // conditional orders are not in this answer and never will be: until one triggers it exists only in
        // the algo store, so a connector reading open orders from here alone reports an account with no
        // stop losses on it - silently, as an empty list rather than an error
        var algoResult = await LoadOpenAlgoOrdersAsync();
        if (!algoResult.IsSuccess)
            return UserResult.From(algoResult, default(IReadOnlyCollection<OrderModel>));

        var orders = result.Data.Concat(algoResult.Data).ToArray();

        this.Trace("done, {count} orders loaded", orders.Length);

        return UserResult.Ok<IReadOnlyCollection<OrderModel>?>(orders);
    }

    /// <summary>
    /// Loads the open conditional orders, across all symbols.
    /// </summary>
    /// <remarks>
    /// A separate endpoint because a conditional order is kept in a separate store until it triggers, which
    /// is not a detail of this provider but of the exchange: an order placed through the algo endpoint is
    /// absent from the ordinary open-orders answer entirely, and appears among ordinary orders only once the
    /// trigger has fired and created one.
    /// </remarks>
    /// <returns>A result carrying the open conditional orders.</returns>
    private async Task<UserResult<IReadOnlyCollection<OrderModel>>> LoadOpenAlgoOrdersAsync()
    {
        var result = await algoOrderRequestFactory
            .New(config.HttpApi)
            .Get("/fapi/v1/openAlgoOrders")
            .ReceiveWindow()
            .Sign(signatureService)
            .WithRateDelay1M(rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers)
            .AsUserResultAsync<IReadOnlyCollection<OrderModel?>>();

        if (!result.IsSuccess)
        {
            if (result.IsFailure)
                this.Debug("algo failure: {result}", result);

            return UserResult.From(result, (IReadOnlyCollection<OrderModel>)[]);
        }

        // the converter returns null for a record an ordinary order already accounts for, and the element
        // type has to be nullable for that to survive the read - a collection of non-nullable elements keeps
        // the null and hands a caller an entry with nothing in it
        return UserResult.Ok<IReadOnlyCollection<OrderModel>>(result.Data.OfType<OrderModel>().ToArray());
    }

    /// <summary>
    /// Loads orders for a symbol: the most recent page when <paramref name="since"/> is null, or the full order
    /// history from that moment onward otherwise.
    /// </summary>
    /// <param name="symbol">The instrument symbol to load orders for.</param>
    /// <param name="since">The timestamp to load orders since, in Unix milliseconds, or null for the latest page.</param>
    /// <returns>A result carrying the orders, or a failure status if they could not be loaded.</returns>
    public async Task<UserResult<IReadOnlyCollection<OrderModel>?>> LoadOrdersAsync(string symbol, long? since)
    {
        var ordinary = since is null
            ? await LoadLatestOrdersAsync(symbol)
            : await LoadOrderHistoryAsync(symbol, since.Value);

        if (!ordinary.IsSuccess)
            return ordinary;

        // merged here rather than inside either branch, so the two entry points cannot drift apart on
        // whether conditional orders are included
        var algo = await LoadAlgoOrderHistoryAsync(symbol, since);
        if (!algo.IsSuccess)
            return UserResult.From(algo, default(IReadOnlyCollection<OrderModel>));

        return UserResult.Ok<IReadOnlyCollection<OrderModel>?>(ordinary.Data.NotNull().Concat(algo.Data).ToArray());
    }

    /// <summary>Returns the highest order id in a page, as the cursor to continue a forward walk from.</summary>
    /// <param name="orders">The page just read.</param>
    /// <returns>The highest id, or null when the page carries no id that parses.</returns>
    /// <remarks>
    /// Ids are carried as strings and compared as numbers, because that is what they are: comparing them
    /// as text makes "9" larger than "10" and stalls the walk on the wrong record.
    /// </remarks>
    private static string? HighestOrderId(IEnumerable<OrderModel> orders)
    {
        string? highest = null;
        var highestValue = long.MinValue;

        foreach (var order in orders)
        {
            if (!long.TryParse(order.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                continue;

            if (value <= highestValue)
                continue;

            highestValue = value;
            highest = order.Id;
        }

        return highest;
    }

    /// <summary>
    /// Loads the conditional orders for a symbol, which the ordinary history does not contain.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only the ones an ordinary order does not already account for survive the read: a conditional order
    /// that triggered became an ordinary order under the same client id, and the converter drops the
    /// record. So this adds exactly the history the other endpoint cannot have - conditional orders that
    /// were cancelled, rejected or expired without ever firing, and the ones still waiting.
    /// </para>
    /// <para>
    /// One request, not a windowed walk. The ordinary history is paged through seven-day windows because it
    /// can run to thousands of orders; the conditional store holds what an account has open and what it
    /// recently closed, and the exchange documents no window limit on it. If that assumption ever fails it
    /// fails visibly - as a page filled to <see cref="OrderQueryLimit"/> - rather than as silence.
    /// </para>
    /// </remarks>
    /// <param name="symbol">The instrument symbol to load conditional orders for.</param>
    /// <param name="since">The timestamp to load from, in Unix milliseconds, or null for the latest page.</param>
    /// <returns>A result carrying the conditional orders.</returns>
    private async Task<UserResult<IReadOnlyCollection<OrderModel>>> LoadAlgoOrderHistoryAsync(
        string symbol,
        long? since
    )
    {
        var request = algoOrderRequestFactory
            .New(config.HttpApi)
            .Get("/fapi/v1/allAlgoOrders")
            .Param("symbol", symbol)
            .Param("limit", OrderQueryLimit);

        if (since is not null)
            request = request.Param("startTime", since.Value);

        var result = await request
            .ReceiveWindow()
            .Sign(signatureService)
            .WithRateDelay1M(rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers)
            .AsUserResultAsync<IReadOnlyCollection<OrderModel?>>();

        if (!result.IsSuccess)
        {
            if (result.IsFailure)
                this.Debug("algo history failure: {result}", result);

            return UserResult.From(result, (IReadOnlyCollection<OrderModel>)[]);
        }

        var orders = result.Data.OfType<OrderModel>().ToArray();

        if (result.Data.Count >= OrderQueryLimit)
            this.Warn<string, int>(
                "{symbol} conditional history filled the page at {limit}, so older ones were not read",
                symbol,
                OrderQueryLimit
            );

        return UserResult.Ok<IReadOnlyCollection<OrderModel>>(orders);
    }

    /// <summary>
    /// Loads trades for a symbol: the most recent page when <paramref name="since"/> is null, or the full trade
    /// history from that moment onward otherwise.
    /// </summary>
    /// <param name="symbol">The instrument symbol to load trades for.</param>
    /// <param name="since">The timestamp to load trades since, in Unix milliseconds, or null for the latest page.</param>
    /// <returns>A result carrying the trades, or a failure status if they could not be loaded.</returns>
    public async Task<UserResult<IReadOnlyCollection<TradeModel>?>> LoadTradesAsync(string symbol, long? since)
    {
        if (since is null)
            return await LoadLatestTradesAsync(symbol);

        return await LoadTradeHistoryAsync(symbol, since.Value);
    }

    /// <summary>
    /// Loads the most recent page of orders for a symbol, up to <see cref="OrderQueryLimit"/> orders.
    /// </summary>
    /// <param name="symbol">The instrument symbol to load orders for.</param>
    /// <returns>A result carrying the orders, or a failure status if they could not be loaded.</returns>
    private async Task<UserResult<IReadOnlyCollection<OrderModel>?>> LoadLatestOrdersAsync(string symbol)
    {
        var result = await getOrderRequestFactory
            .New(config.HttpApi)
            .Get("/fapi/v1/allOrders")
            .Param("symbol", symbol)
            .Param("limit", OrderQueryLimit)
            .ReceiveWindow()
            .Sign(signatureService)
            .WithRateDelay1M(rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
            .AsUserResultAsync<IReadOnlyCollection<OrderModel>>();

        if (!result.IsSuccess)
        {
            if (result.IsFailure)
                this.Debug("failure: {result}", result);

            return UserResult.From(result, default(IReadOnlyCollection<OrderModel>));
        }

        this.Trace("done, {count} orders loaded", result.Data.Count);

        return UserResult.Ok<IReadOnlyCollection<OrderModel>?>(result.Data);
    }

    /// <summary>
    /// Loads the full order history for a symbol from the given moment onward, paging by
    /// <see cref="OrderQueryWindow"/>-sized time windows and, once a window returns a full
    /// <see cref="OrderQueryLimit"/> page, switching to cursor-based paging by order id to drain the remainder.
    /// </summary>
    /// <param name="symbol">The instrument symbol to load orders for.</param>
    /// <param name="since">The timestamp to load orders since, in Unix milliseconds.</param>
    /// <returns>A result carrying the orders, or a failure status if any page could not be loaded.</returns>
    private async Task<UserResult<IReadOnlyCollection<OrderModel>?>> LoadOrderHistoryAsync(string symbol, long since)
    {
        var orders = new Dictionary<string, OrderModel>();
        var (startTime, endTime) = ResolveHistoryBounds(since);
        var start = startTime.ToUnixTimeMilliseconds();
        var end = endTime.ToUnixTimeMilliseconds();
        string? fromOrder = null;

        while (start < end)
        {
            var until = Math.Min(start + OrderQueryWindow, end);

            var chunkResult = await getOrderRequestFactory
                .New(config.HttpApi)
                .Get("/fapi/v1/allOrders")
                .Param("symbol", symbol)
                .Param("limit", OrderQueryLimit)
                .Param("startTime", start)
                .Param("endTime", until)
                .ReceiveWindow()
                .Sign(signatureService)
                .WithRateDelay1M(rateLimiter)
                .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
                .AsUserResultAsync<IReadOnlyCollection<OrderModel>>();

            if (!chunkResult.IsSuccess)
            {
                if (chunkResult.IsFailure)
                    this.Debug("failure: {result}", chunkResult);

                return chunkResult;
            }

            this.Trace("chunk done, {count} orders loaded, merge", chunkResult.Data.Count);
            UserProviderHelper.MergeOrders(orders, chunkResult.Data);

            if (chunkResult.Data.Count == OrderQueryLimit)
            {
                // the highest id in the page, not its last element. Taking the last one assumed the venue
                // returns a page sorted ascending, which it documents nowhere and which this module could
                // not have noticed being wrong: an unsorted page does not fail, it advances the cursor to
                // whatever happened to land last and silently skips everything above it. The maximum is
                // the same value on a sorted page and the correct one on any other.
                fromOrder = HighestOrderId(chunkResult.Data);
                this.Trace<string?>("chunk limit reached, switch to cursor based load from {orderId}", fromOrder);
                break;
            }

            // update to load next interval
            start += OrderQueryWindow;
        }

        while (fromOrder is not null)
        {
            var chunkResult = await getOrderRequestFactory
                .New(config.HttpApi)
                .Get("/fapi/v1/allOrders")
                .Param("symbol", symbol)
                .Param("limit", OrderQueryLimit)
                .Param("orderId", fromOrder)
                .ReceiveWindow()
                .Sign(signatureService)
                .WithRateDelay1M(rateLimiter)
                .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
                .AsUserResultAsync<IReadOnlyCollection<OrderModel>>();

            if (!chunkResult.IsSuccess)
            {
                if (chunkResult.IsFailure)
                    this.Debug("failure: {result}", chunkResult);

                return chunkResult;
            }

            var chunkData = chunkResult.Data.Where(x => x.CreatedAt <= end).ToArray();
            this.Trace("chunk done, {count} orders loaded, merge", chunkData.Length);
            UserProviderHelper.MergeOrders(orders, chunkData);

            if (chunkData.Length == OrderQueryLimit)
                // update to load next chunk if limit is reached by related orders
                fromOrder = chunkData.LastOrDefault()?.Id;
            else
                // break - all related orders loaded
                break;
        }

        this.Trace("done, {count} orders loaded", orders.Count);

        return UserResult.Ok<IReadOnlyCollection<OrderModel>?>(orders.Values);
    }

    /// <summary>
    /// Loads the most recent page of trades for a symbol, up to <see cref="TradeQueryLimit"/> trades.
    /// </summary>
    /// <param name="symbol">The instrument symbol to load trades for.</param>
    /// <returns>A result carrying the trades, or a failure status if they could not be loaded.</returns>
    private async Task<UserResult<IReadOnlyCollection<TradeModel>?>> LoadLatestTradesAsync(string symbol)
    {
        // bounded by time, not by limit. This endpoint truncates a page from the *start* of the window,
        // measured 2026-09-19: asking for five trades returns the five oldest, where the order endpoint's
        // limit returns the newest. So a page cap cannot select recency here, and asking for "the latest
        // page" with a limit alone returns the earliest one on any account busy enough to fill it - a full
        // page of real trades, which nothing downstream can tell from the right one.
        //
        // The window is what selects recency instead. An hour is far more than this path needs: it exists
        // to attach realised PnL to a fill that has just happened, and a fill older than that has already
        // been loaded by the history path.
        var since = timeProvider.Now.ToUnixTimeMilliseconds() - LatestTradesWindow;

        var result = await getTradeRequestFactory
            .New(config.HttpApi)
            .Get("/fapi/v1/userTrades")
            .Param("symbol", symbol)
            .Param("startTime", since)
            .Param("limit", TradeQueryLimit)
            .ReceiveWindow()
            .Sign(signatureService)
            .WithRateDelay1M(rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
            .AsUserResultAsync<IReadOnlyCollection<TradeModel>>();

        if (!result.IsSuccess)
        {
            if (result.IsFailure)
                this.Debug("failure: {result}", result);

            return UserResult.From(result, default(IReadOnlyCollection<TradeModel>));
        }

        this.Trace("done, {count} trades loaded", result.Data.Count);

        return UserResult.Ok<IReadOnlyCollection<TradeModel>?>(result.Data);
    }

    /// <summary>
    /// Loads the full trade history for a symbol from the given moment onward, paging by
    /// <see cref="TradeQueryWindow"/>-sized time windows and, once a window returns a full
    /// <see cref="TradeQueryLimit"/> page, switching to cursor-based paging by trade id to drain the remainder.
    /// </summary>
    /// <param name="symbol">The instrument symbol to load trades for.</param>
    /// <param name="since">The timestamp to load trades since, in Unix milliseconds.</param>
    /// <returns>A result carrying the trades, or a failure status if any page could not be loaded.</returns>
    private async Task<UserResult<IReadOnlyCollection<TradeModel>?>> LoadTradeHistoryAsync(string symbol, long since)
    {
        var trades = new Dictionary<string, TradeModel>();
        var (startTime, endTime) = ResolveHistoryBounds(since);
        var start = startTime.ToUnixTimeMilliseconds();
        var end = endTime.ToUnixTimeMilliseconds();
        string? fromTrade = null;

        while (start < end)
        {
            var until = Math.Min(start + TradeQueryWindow, end);
            var chunkResult = await getTradeRequestFactory
                .New(config.HttpApi)
                .Get("/fapi/v1/userTrades")
                .Param("symbol", symbol)
                .Param("limit", TradeQueryLimit)
                .Param("startTime", start)
                .Param("endTime", until)
                .ReceiveWindow()
                .Sign(signatureService)
                .WithRateDelay1M(rateLimiter)
                .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
                .AsUserResultAsync<IReadOnlyCollection<TradeModel>>();

            if (!chunkResult.IsSuccess)
            {
                if (chunkResult.IsFailure)
                    this.Debug("failure: {result}", chunkResult);

                return chunkResult;
            }

            this.Trace("chunk done, {count} trades loaded, merge", chunkResult.Data.Count);
            UserProviderHelper.MergeTrades(trades, chunkResult.Data);

            if (chunkResult.Data.Count == TradeQueryLimit)
            {
                // this assumes, that trades are sorted!
                fromTrade = chunkResult.Data.Last().Id;
                this.Trace<string?>("chunk limit reached, switch to cursor based load from trade {tradeId}", fromTrade);
                break;
            }

            // update to load next interval
            start += TradeQueryWindow;
        }

        while (fromTrade is not null)
        {
            var chunkResult = await getTradeRequestFactory
                .New(config.HttpApi)
                .Get("/fapi/v1/userTrades")
                .Param("symbol", symbol)
                .Param("limit", TradeQueryLimit)
                .Param("fromId", fromTrade)
                .ReceiveWindow()
                .Sign(signatureService)
                .WithRateDelay1M(rateLimiter)
                .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
                .AsUserResultAsync<IReadOnlyCollection<TradeModel>>();

            if (!chunkResult.IsSuccess)
            {
                if (chunkResult.IsFailure)
                    this.Debug("failure: {result}", chunkResult);

                return chunkResult;
            }

            var chunkData = chunkResult.Data.Where(x => x.Moment <= end).ToArray();
            this.Trace("chunk done, {count} trades loaded, merge", chunkData.Length);
            UserProviderHelper.MergeTrades(trades, chunkData);

            if (chunkData.Length == TradeQueryLimit)
                // update to load next chunk if limit is reached by related trades
                fromTrade = chunkData.LastOrDefault()?.Id;
            else
                // break - all related orders loaded
                break;
        }

        this.Trace("done, {count} trades loaded", trades.Count);

        return UserResult.Ok<IReadOnlyCollection<TradeModel>?>(trades.Values);
    }

    /// <summary>
    /// Widens the requested history range into safe query bounds, padding the lower bound backwards and using
    /// the current time as the upper bound, to tolerate clock skew between this process and the exchange.
    /// </summary>
    /// <param name="since">The requested history start, in Unix milliseconds.</param>
    /// <returns>The widened start and end instants of the history range.</returns>
    private (Instant min, Instant max) ResolveHistoryBounds(long since)
    {
        var now = timeProvider.Now;
        var instant = Instant.FromUnixTimeMilliseconds(since);

        return UserProviderHelper.ResolveHistoryBounds(instant, now);
    }
}
