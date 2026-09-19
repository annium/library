using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Annium.Data.Tables;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Abstractions.Domain.User.Operations;
using Annium.Finance.Providers.Abstractions.Domain.User.Requests;
using Annium.Finance.Providers.Core.Shared;
using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Core.User;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.User.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Contracts.Domain;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Services;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User;

/// <summary>
/// Binance USD-M futures implementation of <see cref="IUserConnector"/>. Reloads account state (assets and
/// positions), open orders and trades through composite/keyed loaders, streams order updates from the user data
/// websocket, and sends leverage/order management commands over signed REST requests.
/// </summary>
internal class UserConnector : UserConnectorBase, IUserConnector
{
    /// <summary>The resolved user connector configuration.</summary>
    private readonly UserConfig _config;

    /// <summary>Builds the query parameters for order management requests.</summary>
    private readonly QueryProcessor _queryProcessor;

    /// <summary>Signs outgoing REST requests with the account's API secret.</summary>
    private readonly ISignatureService _signatureService;

    /// <summary>Factory for requests against the change-leverage endpoint.</summary>
    private readonly IHttpRequestFactory _setLeverageRequestFactory;

    /// <summary>Factory for requests against the place-order endpoint.</summary>
    private readonly IHttpRequestFactory _initOrderRequestFactory;

    /// <summary>Factory for requests against the modify-order endpoint.</summary>
    private readonly IHttpRequestFactory _modifyOrderRequestFactory;

    /// <summary>Factory for requests against the cancel-order endpoint.</summary>
    private readonly IHttpRequestFactory _cancelOrderRequestFactory;

    /// <summary>Factory for requests against the cancel-all-orders endpoint.</summary>
    private readonly IHttpRequestFactory _cancelAllOrdersRequestFactory;

    /// <summary>The request factory for the conditional ("algo") order endpoints.</summary>
    /// <remarks>
    /// Separate from the ordinary order factory because it carries a different serializer: the algo
    /// endpoints answer under their own field names, which one converter cannot share with the other.
    /// </remarks>
    private readonly IHttpRequestFactory _algoOrderRequestFactory;

    /// <summary>Limits request weight against the exchange's rate limits.</summary>
    private readonly IRateLimiter _rateLimiter;

    /// <summary>Reloads the account context (assets and positions).</summary>
    private readonly ICompositeLoader<UserContext> _contextLoader;

    /// <summary>Reloads the currently open orders.</summary>
    private readonly ICompositeLoader<IReadOnlyCollection<OrderModel>> _ordersLoader;

    /// <summary>Reloads trades for a symbol, keyed by symbol and the timestamp to load trades since.</summary>
    private readonly IKeyedLoader<string, long, IReadOnlyCollection<TradeModel>> _tradesLoader;

    /// <summary>The user data websocket stream.</summary>
    private readonly IUserStream _userStream;

    /// <summary>Deserializes <c>ORDER_TRADE_UPDATE</c> user data stream messages.</summary>
    private readonly ISerializer<ReadOnlyMemory<byte>> _orderUpdateEventSerializer;

    /// <summary>Reads the user data stream's conditional order update event.</summary>
    private readonly ISerializer<ReadOnlyMemory<byte>> _algoUpdateEventSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnector"/> class, wiring the context/orders/trades
    /// loaders and the user data stream into the connector's lifecycle.
    /// </summary>
    /// <param name="config">The resolved user connector configuration.</param>
    /// <param name="provider">The user provider used to load account state, orders and trades.</param>
    /// <param name="queryProcessor">Builds the query parameters for order management requests.</param>
    /// <param name="signatureService">Signs outgoing REST requests.</param>
    /// <param name="setLeverageRequestFactory">Factory for requests against the change-leverage endpoint.</param>
    /// <param name="initOrderRequestFactory">Factory for requests against the place-order endpoint.</param>
    /// <param name="modifyOrderRequestFactory">Factory for requests against the modify-order endpoint.</param>
    /// <param name="cancelOrderRequestFactory">Factory for requests against the cancel-order endpoint.</param>
    /// <param name="algoOrderRequestFactory">The request factory for the conditional order endpoints.</param>
    /// <param name="cancelAllOrdersRequestFactory">Factory for requests against the cancel-all-orders endpoint.</param>
    /// <param name="rateLimiter">Limits request weight against the exchange's rate limits.</param>
    /// <param name="contextLoader">Loader that reloads the account context (assets and positions).</param>
    /// <param name="ordersLoader">Loader that reloads the currently open orders.</param>
    /// <param name="tradesLoader">Loader that reloads trades for a symbol.</param>
    /// <param name="userStream">The user data websocket stream.</param>
    /// <param name="algoUpdateEventSerializer">Reads the user data stream's conditional order update event.</param>
    /// <param name="orderUpdateEventSerializer">Deserializes <c>ORDER_TRADE_UPDATE</c> user data stream messages.</param>
    /// <param name="reporter">Reports connector status transitions.</param>
    /// <param name="monitor">Monitors connector status.</param>
    /// <param name="disposable">Accumulates cleanup actions for the connector's lifetime.</param>
    /// <param name="logger">The logger.</param>
    public UserConnector(
        UserConfig config,
        IUserProvider provider,
        QueryProcessor queryProcessor,
        ISignatureService signatureService,
        IHttpRequestFactory setLeverageRequestFactory,
        IHttpRequestFactory initOrderRequestFactory,
        IHttpRequestFactory modifyOrderRequestFactory,
        IHttpRequestFactory cancelOrderRequestFactory,
        IHttpRequestFactory cancelAllOrdersRequestFactory,
        IHttpRequestFactory algoOrderRequestFactory,
        IRateLimiter rateLimiter,
        ICompositeLoader<UserContext> contextLoader,
        ICompositeLoader<IReadOnlyCollection<OrderModel>> ordersLoader,
        IKeyedLoader<string, long, IReadOnlyCollection<TradeModel>> tradesLoader,
        IUserStream userStream,
        ISerializer<ReadOnlyMemory<byte>> orderUpdateEventSerializer,
        ISerializer<ReadOnlyMemory<byte>> algoUpdateEventSerializer,
        IStatusReporter reporter,
        IStatusMonitor monitor,
        AsyncDisposableBox disposable,
        ILogger logger
    )
        : base(config.GetSettings(), provider, reporter, monitor, ConnectorDelivery.Buffered, disposable, logger)
    {
        _config = config;
        _queryProcessor = queryProcessor;
        _signatureService = signatureService;
        _setLeverageRequestFactory = setLeverageRequestFactory;
        _initOrderRequestFactory = initOrderRequestFactory;
        _modifyOrderRequestFactory = modifyOrderRequestFactory;
        _cancelOrderRequestFactory = cancelOrderRequestFactory;
        _cancelAllOrdersRequestFactory = cancelAllOrdersRequestFactory;
        _algoOrderRequestFactory = algoOrderRequestFactory;
        _rateLimiter = rateLimiter;

        // context
        _contextLoader = contextLoader;
        _contextLoader.OnData += HandleContext;
        Disposable += () => _contextLoader.OnData -= HandleContext;

        // orders
        _ordersLoader = ordersLoader;
        _ordersLoader.OnData += HandleOrders;
        Disposable += () => _ordersLoader.OnData -= HandleOrders;

        // trades
        _tradesLoader = tradesLoader;
        _tradesLoader.OnData += HandleTrades;
        Disposable += () => _tradesLoader.OnData -= HandleTrades;

        // user stream
        _userStream = userStream;
        _userStream.OnConnected += HandleConnected;
        Disposable += () => _userStream.OnConnected -= HandleConnected;

        _userStream.OnDisconnected += HandleDisconnected;
        Disposable += () => _userStream.OnDisconnected -= HandleDisconnected;

        _userStream.OnMessage += HandleMessage;
        Disposable += () => _userStream.OnMessage -= HandleMessage;

        _orderUpdateEventSerializer = orderUpdateEventSerializer;
        _algoUpdateEventSerializer = algoUpdateEventSerializer;
    }

    /// <summary>
    /// Sets the leverage used for a position, flooring the leverage to a whole number as required by the
    /// exchange, and reports whether the account ended up with it.
    /// </summary>
    /// <remarks>
    /// It used to report success whatever came back, which left a caller sizing positions against a
    /// leverage the account does not have. Two things are reported now: the exchange's own refusal, and -
    /// where the exchange accepts the change without applying it - a refusal of the connector's own, from
    /// comparing the leverage that came back with the one asked for. An answer that carries the resulting
    /// state is worth comparing against the request; without that comparison, "accepted but unchanged" is
    /// indistinguishable from success.
    /// </remarks>
    /// <param name="position">The position to change leverage for.</param>
    /// <param name="leverage">The leverage to set.</param>
    /// <returns>An OK result once the account reports the requested leverage; a failure otherwise.</returns>
    public async ValueTask<UserResult> SetLeverageAsync(PositionModel position, decimal leverage)
    {
        if (Status is not ConnectorStatus.Connected)
        {
            this.Warn("{id} skip for {position} -> {leverage} - not connected", Id, position, leverage);
            return UserResult.New(UserOperationStatus.NotConnected);
        }

        var target = leverage.FloorInt32();

        var result = await _setLeverageRequestFactory
            .New(_config.HttpApi)
            .Post("/fapi/v1/leverage")
            .Param("symbol", position.Symbol)
            .Param("leverage", target)
            .ReceiveWindow()
            .Sign(_signatureService)
            .WithRateDelay1M(_rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
            .AsUserResultAsync<LeverageResponse>();

        HandleTradeResult(result.IsSuccess);

        if (result.IsFailure)
        {
            this.Warn<string, string, int>(
                "{id} leverage of {position} -> {leverage} refused",
                Id,
                position.Symbol,
                target
            );

            return UserResult.From(result);
        }

        // the response carries the leverage the account ended up with, so an acceptance that changed
        // nothing is visible here and nowhere else: to a caller reading only the status it looks exactly
        // like the change having been applied
        if (result.Data is not { } data || data.Leverage != target)
        {
            var applied = result.Data is { } current ? current.Leverage.ToString() : "nothing";
            this.Warn<string, string, int, string>(
                "{id} leverage of {position} -> {leverage} not applied, account reports {applied}",
                Id,
                position.Symbol,
                target,
                applied
            );

            return UserResult.New(
                UserOperationStatus.UnknownError,
                $"Leverage was accepted but not applied: asked for {target}, account reports {applied}"
            );
        }

        return UserResult.Ok();
    }

    /// <summary>
    /// Places a new order.
    /// </summary>
    /// <param name="request">The order parameters.</param>
    /// <returns>A result carrying the placed order on success, or null data with a non-success status on failure.</returns>
    public async ValueTask<UserResult<OrderModel?>> InitOrderAsync(IInitOrderRequest request)
    {
        if (Status is not ConnectorStatus.Connected)
        {
            this.Warn("{id} skip for {request} - not connected", Id, request);
            return UserResult.New(UserOperationStatus.NotConnected, default(OrderModel));
        }

        // the four conditional types left the ordinary endpoint on 2025-12-09 and are refused there with
        // -4120. Routing is decided here, once, from the order's own type
        if (QueryProcessor.IsConditional(request.Type))
            return await InitAlgoOrderAsync(request);

        var queryResult = _queryProcessor.BuildInitOrderQuery(request);
        if (!queryResult.IsSuccess)
        {
            this.Warn("{id} query processing failed: {result}", Id, queryResult);
            return UserResult.From(queryResult, default(OrderModel));
        }

        var result = await _initOrderRequestFactory
            .New(_config.HttpApi)
            .Post("/fapi/v1/order")
            .Params(queryResult.Data)
            .ReceiveWindow()
            .Sign(_signatureService)
            .WithRateDelay1M(_rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
            .AsUserResultAsync<OrderModel>();

        HandleTradeResult(result.IsSuccess);

        return result;
    }

    /// <summary>
    /// Places a conditional order through <c>POST /fapi/v1/algoOrder</c>.
    /// </summary>
    /// <remarks>
    /// The connector is already connected and the request already routed here by type, so this repeats
    /// neither check. What it does not share with the ordinary placement is the query - four parameter names
    /// differ - and the serializer, since the answer comes back under the algo field names.
    /// </remarks>
    /// <param name="request">The order parameters.</param>
    /// <returns>A result carrying the placed order, or a non-success status.</returns>
    private async ValueTask<UserResult<OrderModel?>> InitAlgoOrderAsync(IInitOrderRequest request)
    {
        var queryResult = _queryProcessor.BuildInitAlgoOrderQuery(request);
        if (!queryResult.IsSuccess)
        {
            this.Warn("{id} algo query processing failed: {result}", Id, queryResult);
            return UserResult.From(queryResult, default(OrderModel));
        }

        var result = await _algoOrderRequestFactory
            .New(_config.HttpApi)
            .Post("/fapi/v1/algoOrder")
            .Params(queryResult.Data)
            .ReceiveWindow()
            .Sign(_signatureService)
            .WithRateDelay1M(_rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
            .AsUserResultAsync<OrderModel>();

        HandleTradeResult(result.IsSuccess);

        return result;
    }

    /// <summary>
    /// Modifies an existing order. Binance's amend endpoint only supports limit orders; for any other order type
    /// this cancels the existing order and places a new one with the requested parameters instead.
    /// </summary>
    /// <param name="request">The modification parameters, including the order being modified.</param>
    /// <returns>A result carrying the resulting order on success, or null data with a non-success status on failure.</returns>
    public async ValueTask<UserResult<OrderModel?>> ModifyOrderAsync(IModifyOrderRequest request)
    {
        if (Status is not ConnectorStatus.Connected)
        {
            this.Warn("{id} skip for {request} - not connected", Id, request);
            return UserResult.New(UserOperationStatus.NotConnected, default(OrderModel));
        }

        // non limit orders can only be canceled and created from scratch
        if (request.Order.Type is not OrderType.Limit)
        {
            // try cancel order
            var order = request.Order;
            var cancelRequest = RequestBuilder.CancelOrder(order);
            var cancelResult = await CancelOrderAsync(cancelRequest);
            if (cancelResult.IsFailure)
            {
                if (!cancelResult.IsAborted)
                    this.Warn("{id} cancel of order {order} failed: {result}", Id, order, cancelResult);
                return UserResult.From(cancelResult, default(OrderModel));
            }

            var initRequest = request.ToInitOrderRequest();
            var initResult = await InitOrderAsync(initRequest);

            return initResult;
        }

        var queryResult = _queryProcessor.BuildModifyOrderQuery(request);
        if (!queryResult.IsSuccess)
        {
            this.Warn("{id} query processing failed: {result}", Id, queryResult);
            return UserResult.From(queryResult, default(OrderModel));
        }

        var result = await _modifyOrderRequestFactory
            .New(_config.HttpApi)
            .Put("/fapi/v1/order")
            .Params(queryResult.Data)
            .ReceiveWindow()
            .Sign(_signatureService)
            .WithRateDelay1M(_rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
            .AsUserResultAsync<OrderModel>();

        HandleTradeResult(result.IsSuccess);

        return result;
    }

    /// <summary>
    /// Cancels an existing order.
    /// </summary>
    /// <param name="request">Identifies the order to cancel.</param>
    /// <returns>A result indicating whether the cancellation succeeded.</returns>
    public async ValueTask<UserResult> CancelOrderAsync(ICancelOrderRequest request)
    {
        if (Status is not ConnectorStatus.Connected)
        {
            this.Warn("{id} skip for {order} - not connected", Id, request);
            return UserResult.New(UserOperationStatus.NotConnected);
        }

        // the store an order lives in decides the endpoint that can cancel it, and the request carries its
        // type for exactly this reason
        if (QueryProcessor.IsConditional(request.Type))
            return await CancelAlgoOrderAsync(request);

        var queryResult = _queryProcessor.BuildCancelOrderQuery(request);
        if (!queryResult.IsSuccess)
        {
            this.Warn("{id} query processing failed: {result}", Id, queryResult);
            return UserResult.From(queryResult);
        }

        var result = await _cancelOrderRequestFactory
            .New(_config.HttpApi)
            .Delete("/fapi/v1/order")
            .Params(queryResult.Data)
            .ReceiveWindow()
            .Sign(_signatureService)
            .WithRateDelay1M(_rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
            .AsUserResultAsync<CancelOrderResponse>();

        HandleTradeResult(result.IsSuccess);

        return UserResult.From(result);
    }

    /// <summary>
    /// Cancels a conditional order through <c>DELETE /fapi/v1/algoOrder</c>.
    /// </summary>
    /// <remarks>
    /// The answer is the <c>{code, msg}</c> envelope rather than the cancelled order, and it spells the code
    /// as the string <c>"200"</c> on success - which is why the shared result converter reads that field in
    /// either form.
    /// </remarks>
    /// <param name="request">The order to cancel.</param>
    /// <returns>A result indicating whether the cancellation succeeded.</returns>
    private async ValueTask<UserResult> CancelAlgoOrderAsync(ICancelOrderRequest request)
    {
        var queryResult = _queryProcessor.BuildCancelAlgoOrderQuery(request);
        if (!queryResult.IsSuccess)
        {
            this.Warn("{id} algo cancel query processing failed: {result}", Id, queryResult);
            return UserResult.From(queryResult);
        }

        var result = await _algoOrderRequestFactory
            .New(_config.HttpApi)
            .Delete("/fapi/v1/algoOrder")
            .Params(queryResult.Data)
            .ReceiveWindow()
            .Sign(_signatureService)
            .WithRateDelay1M(_rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
            .AsUserResultAsync<OperationResult>();

        HandleTradeResult(result.IsSuccess);

        return UserResult.From(result);
    }

    /// <summary>
    /// Cancels all open orders for the given symbol.
    /// </summary>
    /// <param name="symbol">The instrument symbol to cancel orders for.</param>
    /// <returns>A result indicating whether the cancellation succeeded.</returns>
    public async ValueTask<UserResult> CancelAllOrdersAsync(string symbol)
    {
        if (Status is not ConnectorStatus.Connected)
        {
            this.Warn<string, string>("{id} skip for {symbol} - not connected", Id, symbol);
            return UserResult.New(UserOperationStatus.NotConnected);
        }

        var queryResult = _queryProcessor.BuildCancelAllOrdersQuery(symbol);
        if (!queryResult.IsSuccess)
        {
            this.Warn("{id} query processing failed: {result}", Id, queryResult);
            return UserResult.From(queryResult);
        }

        var result = await _cancelAllOrdersRequestFactory
            .New(_config.HttpApi)
            .Delete("/fapi/v1/allOpenOrders")
            .Params(queryResult.Data)
            .ReceiveWindow()
            .Sign(_signatureService)
            .WithRateDelay1M(_rateLimiter)
            .WithLogFromWithHeaders(this, LogData.Headers | LogData.Response)
            .AsUserResultAsync<OperationResult>();

        HandleTradeResult(result.IsSuccess);

        return UserResult.From(result);
    }

    /// <summary>
    /// Requests an account context reload after a trade command, and additionally requests an open orders reload
    /// if the command failed, since a failure may still have changed order state on the exchange.
    /// </summary>
    /// <param name="isSuccess">Whether the trade command succeeded.</param>
    private void HandleTradeResult(bool isSuccess)
    {
        _contextLoader.Request();

        if (!isSuccess)
        {
            this.Trace<string>("{id} trade failed, request orders load", Id);
            _ordersLoader.Request();
        }
    }

    /// <summary>
    /// Publishes the reloaded account context as full-snapshot <c>Init</c> events on <see cref="IUserConnector.Assets"/> and
    /// <see cref="IUserConnector.Positions"/>.
    /// </summary>
    /// <param name="context">The reloaded account context.</param>
    private void HandleContext(UserContext context)
    {
        Write(ChangeEvent.Init(context.Assets));
        Write(ChangeEvent.Init(context.Positions));
    }

    /// <summary>
    /// Publishes the reloaded open orders as a full-snapshot <c>Init</c> event on <see cref="IUserConnector.Orders"/>.
    /// </summary>
    /// <param name="orders">The reloaded open orders.</param>
    private void HandleOrders(IReadOnlyCollection<OrderModel> orders)
    {
        Write(ChangeEvent.Init(orders));
    }

    /// <summary>
    /// Publishes each reloaded trade individually on <see cref="IUserConnector.Trades"/>.
    /// </summary>
    /// <param name="symbol">The symbol trades were reloaded for.</param>
    /// <param name="since">The timestamp trades were reloaded since.</param>
    /// <param name="items">The reloaded trades.</param>
    private void HandleTrades(string symbol, long since, IReadOnlyCollection<TradeModel> items)
    {
        foreach (var item in items)
            Write(item);
    }

    /// <summary>
    /// Starts the account context and open orders loaders once the user data stream connects.
    /// </summary>
    private void HandleConnected()
    {
        this.Trace<string>("{id} start", Id);

        _contextLoader.Start(true);
        _ordersLoader.Start(true);

        this.Trace<string>("{id} done", Id);
    }

    /// <summary>
    /// Stops the account context and open orders loaders once the user data stream disconnects.
    /// </summary>
    private void HandleDisconnected()
    {
        this.Trace<string>("{id} start", Id);

        _contextLoader.Stop();
        _ordersLoader.Stop();

        this.Trace<string>("{id} done", Id);
    }

    /// <summary>
    /// Handles a raw user data stream message: since the <c>ACCOUNT_UPDATE</c> event carries an incomplete
    /// account snapshot (no leverage on positions), every message triggers an account context reload; messages
    /// that additionally parse as an <see cref="OrderUpdateEvent"/> are forwarded to <see cref="HandleOrderUpdate"/>.
    /// </summary>
    /// <param name="data">The raw message payload.</param>
    private void HandleMessage(ReadOnlyMemory<byte> data)
    {
        // guarded: decoding the payload to a string is the whole message, and arguments are evaluated
        // before the level is looked at - this runs on every message the account stream delivers
        if (LogConfig.IsEnabled(LogLevel.Trace))
            this.Trace<string, string>("{id} handle {msg}", Id, Encoding.UTF8.GetString(data.Span));

        // account info in event is almost useless (and position info lacks leverage value), so request account reload
        _contextLoader.Request();

        // handle order update
        var orderUpdate = _orderUpdateEventSerializer.Deserialize<OrderUpdateEvent?>(data);
        if (orderUpdate is not null)
        {
            HandleOrderUpdate(orderUpdate);
            return;
        }

        // conditional orders are announced on their own event and never by ORDER_TRADE_UPDATE until their
        // trigger fires, so without this a stop loss appears only when the orders loader next polls
        var algoUpdate = _algoUpdateEventSerializer.Deserialize<AlgoUpdateEvent?>(data);
        if (algoUpdate is not null)
            HandleAlgoUpdate(algoUpdate);
    }

    /// <summary>
    /// Publishes an <c>ALGO_UPDATE</c> event on <see cref="IUserConnector.Orders"/>: a <c>Set</c> while the
    /// conditional order is still waiting, a <c>Delete</c> once it is over.
    /// </summary>
    /// <remarks>
    /// A triggered conditional order is deleted rather than updated. The exchange creates an ordinary order
    /// carrying the same client id, and that order announces itself on its own event - so leaving the
    /// conditional record in place would show the caller the order twice, once with its real fill and once
    /// with a record that cannot say whether the book filled or cancelled it.
    /// </remarks>
    /// <param name="e">The conditional order update event.</param>
    private void HandleAlgoUpdate(AlgoUpdateEvent e)
    {
        var order = new OrderModel(
            e.AlgoId,
            e.ClientAlgoId,
            e.Range,
            e.Symbol,
            e.Side,
            e.Type,
            e.TotalQty,
            e.Price,
            e.LevelPrice,
            e.ReduceOnly,
            e.UpdatedAt,
            e.Status,
            0m,
            0m,
            e.UpdatedAt
        );

        if (e.IsSuperseded || e.Status is not (OrderStatus.New or OrderStatus.PartiallyFilled))
            Write(ChangeEvent.Delete(order));
        else
            Write(ChangeEvent.Set(order));
    }

    /// <summary>
    /// Publishes an <c>ORDER_TRADE_UPDATE</c> event on <see cref="IUserConnector.Orders"/> as a <c>Set</c> event while the order
    /// is still open (new or partially filled) and a <c>Delete</c> event once it stops being open. Also requests
    /// a trades reload for the symbol on a (partial) fill, since PnL is not available from the stream event.
    /// </summary>
    /// <param name="e">The order update event.</param>
    private void HandleOrderUpdate(OrderUpdateEvent e)
    {
        if (e.Status is OrderStatus.PartiallyFilled or OrderStatus.Filled)
        {
            // as far as pnl is not available here - request reload by http
            _tradesLoader.Request(e.Symbol);
        }

        var order = new OrderModel(
            e.OrderId,
            e.ClientOrderId,
            e.Range,
            e.Symbol,
            e.Side,
            e.Type,
            e.TotalQty,
            e.Price,
            e.LevelPrice,
            e.ReduceOnly,
            e.CreatedAt,
            e.Status,
            e.ExecutedQty,
            e.ExecutedPrice,
            e.UpdatedAt
        );

        var item = order.Status is OrderStatus.New or OrderStatus.PartiallyFilled
            ? ChangeEvent.Set(order)
            : ChangeEvent.Delete(order);

        Write(item);
    }
}
