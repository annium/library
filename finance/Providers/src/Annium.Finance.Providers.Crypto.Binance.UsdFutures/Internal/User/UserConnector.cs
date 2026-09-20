using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
    /// <summary>How many ended-order notes are kept when no snapshot arrives to retire them.</summary>
    private const int MaxEndedNotes = 1000;

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

    /// <summary>
    /// Ids of orders the stream has reported as over, so that a snapshot requested before they ended
    /// cannot raise them again. See <see cref="HandleOrders"/> for why, and for how they are forgotten.
    /// </summary>
    private readonly HashSet<string> _ended = new();

    /// <summary>The order <see cref="_ended"/> was added to, so the oldest note is the one that goes.</summary>
    private readonly Queue<string> _endedOrder = new();

    /// <summary>Guards <see cref="_ended"/>, written from the stream and read from the loader.</summary>
    private readonly Lock _endedLocker = new();

    /// <summary>Reloads trades for a symbol, keyed by symbol and the timestamp to load trades since.</summary>
    private readonly IKeyedLoader<string, long, IReadOnlyCollection<TradeModel>> _tradesLoader;

    /// <summary>The user data websocket stream.</summary>
    private readonly IUserStream _userStream;

    /// <summary>Deserializes <c>ORDER_TRADE_UPDATE</c> user data stream messages.</summary>
    private readonly ISerializer<ReadOnlyMemory<byte>> _orderUpdateEventSerializer;

    /// <summary>Reads the user data stream's conditional order update event.</summary>
    private readonly ISerializer<ReadOnlyMemory<byte>> _algoUpdateEventSerializer;

    /// <summary>Reads the user data stream's earliest fill notice.</summary>
    private readonly ISerializer<ReadOnlyMemory<byte>> _tradeLiteEventSerializer;

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
    /// <param name="tradeLiteEventSerializer">Reads the user data stream's earliest fill notice.</param>
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
        ISerializer<ReadOnlyMemory<byte>> tradeLiteEventSerializer,
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
        _tradeLiteEventSerializer = tradeLiteEventSerializer;
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
    /// Publishes the reloaded open orders as a full-snapshot <c>Init</c> event on <see cref="IUserConnector.Orders"/>,
    /// with any order the stream has already reported as over left out of it.
    /// </summary>
    /// <remarks>
    /// A snapshot describes the account as it was when the request was sent, and a request sent a
    /// fraction of a second before an order ended comes back still listing it. Published as it arrives,
    /// that raises an order the caller has just been told was cancelled - and since the order is gone
    /// from every later snapshot, nothing ever corrects it. A caller watching a stop loss would go on
    /// believing it was armed.
    ///
    /// Measured on the wire: the cancel left at :39.075, a reload's request at :39.178, the venue's
    /// cancellation event at :39.357, and that reload's answer at :39.448 - a window of a couple of
    /// hundred milliseconds in which the answer is stale but arrives last. It failed one run in four.
    ///
    /// So a terminal stream event is remembered, and the memory is dropped by the first snapshot that
    /// agrees the order is gone. Terminal is final at this venue - nothing un-cancels an order - which
    /// is what makes the newer of the two sources the right one every time.
    /// </remarks>
    /// <param name="orders">The reloaded open orders.</param>
    private void HandleOrders(IReadOnlyCollection<OrderModel> orders)
    {
        Write(ChangeEvent.Init(WithoutOrdersAlreadyOver(orders)));
    }

    /// <summary>
    /// Removes from a snapshot the orders a stream event has already reported as over, and forgets the
    /// ones the snapshot itself no longer lists.
    /// </summary>
    /// <param name="orders">The snapshot as the venue answered it.</param>
    /// <returns>The snapshot without any order already known to be over.</returns>
    private IReadOnlyCollection<OrderModel> WithoutOrdersAlreadyOver(IReadOnlyCollection<OrderModel> orders)
    {
        lock (_endedLocker)
        {
            if (_ended.Count == 0)
                return orders;

            var kept = new List<OrderModel>(orders.Count);
            var stale = new List<string>();
            foreach (var order in orders)
                if (_ended.Contains(order.Id))
                    stale.Add(order.Id);
                else
                    kept.Add(order);

            // an id this snapshot does not mention is one the venue agrees is gone, so the note about it
            // has done its work. Only the ids this very snapshot still claimed are worth carrying on.
            _ended.Clear();
            _endedOrder.Clear();
            foreach (var id in stale)
            {
                _ended.Add(id);
                _endedOrder.Enqueue(id);
            }

            if (stale.Count > 0)
                this.Trace<string, string>(
                    "{id} snapshot still listed {count} order(s) already over",
                    Id,
                    stale.Count.ToString()
                );

            return kept;
        }
    }

    /// <summary>
    /// Notes that the stream has reported an order as over, so nothing older can raise it again.
    /// </summary>
    /// <param name="id">The exchange-assigned id of the order.</param>
    private void NoteOrderIsOver(string id)
    {
        lock (_endedLocker)
        {
            if (!_ended.Add(id))
                return;

            _endedOrder.Enqueue(id);

            // notes are retired by the next snapshot, so in ordinary running there are a handful of them.
            // The cap is for the case where snapshots stop arriving while the stream keeps going - a
            // reload failing against a rate limit does exactly that, for minutes - and nothing would
            // retire anything. A note that old protects nothing anyway: it guards against a snapshot
            // already in flight, and once the loader recovers, the snapshot it sends is newer than every
            // note here and lists none of them.
            while (_endedOrder.Count > MaxEndedNotes)
                _ended.Remove(_endedOrder.Dequeue());
        }
    }

    /// <summary>
    /// Says whether an order has already been reported as over.
    /// </summary>
    /// <remarks>
    /// The stale snapshot is not the only way an order comes back to life. The venue's own events are
    /// not ordered against each other either: a placement event was measured arriving 600ms after the
    /// order was placed, which is long enough to land after the cancellation of the same order when the
    /// two are a second apart - and a caller told an order is cancelled and then told it is new has been
    /// told the wrong thing by exactly the source it trusts most.
    ///
    /// Over is final here. Nothing un-cancels an order and no id is reused, so the report that came
    /// first in time is the one that stands, whichever arrived last.
    /// </remarks>
    /// <param name="id">The exchange-assigned id of the order.</param>
    /// <returns>True when the order has already been reported as over.</returns>
    private bool IsAlreadyOver(string id)
    {
        lock (_endedLocker)
        {
            if (!_ended.Contains(id))
                return false;
        }

        this.Trace<string, string>("{id} ignoring a live report of order {order}, already over", Id, id);

        return true;
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

        // the earliest notice of a fill this venue gives - it precedes the order update reporting the
        // same fill. Nothing is published from it: it carries no commission and no realised PnL, so a
        // trade built from it would have a zero fee, which is not missing data a caller can see but wrong
        // data it cannot. What it buys is starting the reload sooner
        var tradeLite = _tradeLiteEventSerializer.Deserialize<TradeLiteEvent?>(data);
        if (tradeLite is not null)
        {
            _tradesLoader.Request(tradeLite.Symbol);
            return;
        }

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
        {
            NoteOrderIsOver(order.Id);
            Write(ChangeEvent.Delete(order));
        }
        else if (!IsAlreadyOver(order.Id))
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

        if (order.Status is OrderStatus.New or OrderStatus.PartiallyFilled)
        {
            if (!IsAlreadyOver(order.Id))
                Write(ChangeEvent.Set(order));

            return;
        }

        NoteOrderIsOver(order.Id);
        Write(ChangeEvent.Delete(order));
    }
}
