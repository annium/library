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
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.Shared;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User.Contracts.Domain;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User.Services;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User;

/// <summary>
/// Binance spot account connector: keeps account state in step with the venue over the user data stream and
/// periodic reloads, and places, replaces and cancels orders.
/// </summary>
/// <remarks>
/// <para>
/// Every venue fact here comes from §10 and §11 of the provider manifest. Three of them make this something
/// other than the neighbouring venue's connector with the leverage taken out: the account stream is reached
/// by a signed subscription rather than by a key, a change of price is a cancel-and-replace rather than an
/// amendment, and a spot account has no positions to report.
/// </para>
/// </remarks>
internal class UserConnector : UserConnectorBase, IUserConnector
{
    /// <summary>How many ended-order notes are kept when no snapshot arrives to retire them.</summary>
    private const int MaxEndedNotes = 1000;

    /// <summary>The resolved account connection settings.</summary>
    private readonly UserConfig _config;

    /// <summary>Builds the query parameters for order management requests.</summary>
    private readonly QueryProcessor _queryProcessor;

    /// <summary>Signs outgoing REST requests with the account's API secret.</summary>
    private readonly ISignatureService _signatureService;

    /// <summary>Factory for requests against the place-order endpoint.</summary>
    private readonly IHttpRequestFactory _initOrderRequestFactory;

    /// <summary>Factory for requests against the cancel-and-replace endpoint.</summary>
    private readonly IHttpRequestFactory _modifyOrderRequestFactory;

    /// <summary>Factory for requests against the cancel-order endpoint.</summary>
    private readonly IHttpRequestFactory _cancelOrderRequestFactory;

    /// <summary>Factory for requests against the cancel-all-orders endpoint.</summary>
    private readonly IHttpRequestFactory _cancelAllOrdersRequestFactory;

    /// <summary>Limits request weight against the exchange's rate limits.</summary>
    private readonly IRateLimiter _rateLimiter;

    /// <summary>Loader that reloads the account context (balances).</summary>
    private readonly ICompositeLoader<UserContext> _contextLoader;

    /// <summary>Loader that reloads the currently open orders.</summary>
    private readonly ICompositeLoader<IReadOnlyCollection<OrderModel>> _ordersLoader;

    /// <summary>Loader that reloads trades for a symbol.</summary>
    private readonly IKeyedLoader<string, long, IReadOnlyCollection<TradeModel>> _tradesLoader;

    /// <summary>Ids of orders a stream event has already reported as over.</summary>
    private readonly HashSet<string> _ended = new();

    /// <summary>The same ids in arrival order, so the oldest can be dropped when the set is capped.</summary>
    private readonly Queue<string> _endedOrder = new();

    /// <summary>Guards <see cref="_ended"/> and <see cref="_endedOrder"/>.</summary>
    private readonly Lock _endedLocker = new();

    /// <summary>The account's event stream.</summary>
    private readonly IUserStream _userStream;

    /// <summary>Reads the stream's order update event.</summary>
    private readonly ISerializer<ReadOnlyMemory<byte>> _orderUpdateEventSerializer;

    /// <summary>Reads the stream's balance update event.</summary>
    private readonly ISerializer<ReadOnlyMemory<byte>> _accountUpdateEventSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnector"/> class, wiring the loaders and the account
    /// stream into the connector's lifecycle.
    /// </summary>
    /// <param name="config">The resolved account connection settings.</param>
    /// <param name="provider">The user data provider used to load account state.</param>
    /// <param name="queryProcessor">Builds the query parameters for order management requests.</param>
    /// <param name="signatureService">Signs outgoing REST requests.</param>
    /// <param name="initOrderRequestFactory">Factory for requests against the place-order endpoint.</param>
    /// <param name="modifyOrderRequestFactory">Factory for requests against the cancel-and-replace endpoint.</param>
    /// <param name="cancelOrderRequestFactory">Factory for requests against the cancel-order endpoint.</param>
    /// <param name="cancelAllOrdersRequestFactory">Factory for requests against the cancel-all-orders endpoint.</param>
    /// <param name="rateLimiter">Limits request weight against the exchange's rate limits.</param>
    /// <param name="contextLoader">Loader that reloads the account context.</param>
    /// <param name="ordersLoader">Loader that reloads the currently open orders.</param>
    /// <param name="tradesLoader">Loader that reloads trades for a symbol.</param>
    /// <param name="userStream">The account's event stream.</param>
    /// <param name="orderUpdateEventSerializer">Reads the stream's order update event.</param>
    /// <param name="accountUpdateEventSerializer">Reads the stream's balance update event.</param>
    /// <param name="reporter">The status reporter used to publish connection status changes.</param>
    /// <param name="monitor">The status monitor used to detect and recover from stalled connections.</param>
    /// <param name="disposable">The disposable box collecting this connector's cleanup actions.</param>
    /// <param name="logger">The logger instance.</param>
    public UserConnector(
        UserConfig config,
        IUserProvider provider,
        QueryProcessor queryProcessor,
        ISignatureService signatureService,
        IHttpRequestFactory initOrderRequestFactory,
        IHttpRequestFactory modifyOrderRequestFactory,
        IHttpRequestFactory cancelOrderRequestFactory,
        IHttpRequestFactory cancelAllOrdersRequestFactory,
        IRateLimiter rateLimiter,
        ICompositeLoader<UserContext> contextLoader,
        ICompositeLoader<IReadOnlyCollection<OrderModel>> ordersLoader,
        IKeyedLoader<string, long, IReadOnlyCollection<TradeModel>> tradesLoader,
        IUserStream userStream,
        ISerializer<ReadOnlyMemory<byte>> orderUpdateEventSerializer,
        ISerializer<ReadOnlyMemory<byte>> accountUpdateEventSerializer,
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
        _initOrderRequestFactory = initOrderRequestFactory;
        _modifyOrderRequestFactory = modifyOrderRequestFactory;
        _cancelOrderRequestFactory = cancelOrderRequestFactory;
        _cancelAllOrdersRequestFactory = cancelAllOrdersRequestFactory;
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
        _accountUpdateEventSerializer = accountUpdateEventSerializer;
    }

    /// <summary>
    /// Refuses: a spot account holds no leveraged positions, so there is no leverage to set.
    /// </summary>
    /// <remarks>
    /// A refusal rather than an exception, and rather than a success. An exception makes a caller that
    /// works across venues handle this one specially, which is the opposite of what the interface is for.
    /// A success would be the worse failure of the two: a caller told its leverage was applied sizes the
    /// next position against a number the account has never heard of.
    /// </remarks>
    /// <param name="position">The position to change leverage for.</param>
    /// <param name="leverage">The leverage to set.</param>
    /// <returns>A refusal.</returns>
    public ValueTask<UserResult> SetLeverageAsync(PositionModel position, decimal leverage)
    {
        this.Warn<string, string>(
            "{id} refuse leverage change for {symbol} - spot has no leverage",
            Id,
            position.Symbol
        );

        return ValueTask.FromResult(
            UserResult.New(UserOperationStatus.BadRequest, "Spot accounts hold no leveraged positions")
        );
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

        var queryResult = _queryProcessor.BuildInitOrderQuery(request);
        if (!queryResult.IsSuccess)
        {
            this.Warn("{id} query processing failed: {result}", Id, queryResult);
            return UserResult.From(queryResult, default(OrderModel));
        }

        var result = await _initOrderRequestFactory
            .New(_config.HttpApi)
            .Post(Endpoints.InitOrderUriPath)
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
    /// Replaces an existing order with one on the requested terms.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A single cancel-and-replace request rather than a cancel followed by a placement, because this venue
    /// offers one that is evaluated as a unit: sent with the mode that stops on failure, a cancel that does
    /// not succeed leaves the original order where it was rather than leaving the account with neither.
    /// </para>
    /// <para>
    /// This is not an amendment and does not claim to be. The venue's own amend endpoint reduces quantity
    /// and nothing else, so a change of price loses the order's place in the queue whichever way it is
    /// expressed here. Reporting that plainly is the point of using the endpoint that says so.
    /// </para>
    /// </remarks>
    /// <param name="request">The modification parameters, including the order being modified.</param>
    /// <returns>A result carrying the resulting order on success, or null data with a non-success status on failure.</returns>
    public async ValueTask<UserResult<OrderModel?>> ModifyOrderAsync(IModifyOrderRequest request)
    {
        if (Status is not ConnectorStatus.Connected)
        {
            this.Warn("{id} skip for {request} - not connected", Id, request);
            return UserResult.New(UserOperationStatus.NotConnected, default(OrderModel));
        }

        var queryResult = _queryProcessor.BuildModifyOrderQuery(request);
        if (!queryResult.IsSuccess)
        {
            this.Warn("{id} query processing failed: {result}", Id, queryResult);
            return UserResult.From(queryResult, default(OrderModel));
        }

        var result = await _modifyOrderRequestFactory
            .New(_config.HttpApi)
            .Post(Endpoints.ModifyOrderUriPath)
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
    /// <param name="order">Identifies the order to cancel.</param>
    /// <returns>A result indicating whether the cancellation succeeded.</returns>
    public async ValueTask<UserResult> CancelOrderAsync(ICancelOrderRequest order)
    {
        if (Status is not ConnectorStatus.Connected)
        {
            this.Warn("{id} skip for {order} - not connected", Id, order);
            return UserResult.New(UserOperationStatus.NotConnected);
        }

        var queryResult = _queryProcessor.BuildCancelOrderQuery(order);
        if (!queryResult.IsSuccess)
        {
            this.Warn("{id} query processing failed: {result}", Id, queryResult);
            return UserResult.From(queryResult);
        }

        var result = await _cancelOrderRequestFactory
            .New(_config.HttpApi)
            .Delete(Endpoints.CancelOrderUriPath)
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
    /// Cancels every open order on the given symbol.
    /// </summary>
    /// <remarks>
    /// The symbol is required by the venue: there is no unscoped cancel-all here, so a caller wanting the
    /// whole account cleared has to ask symbol by symbol.
    /// </remarks>
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
            .Delete(Endpoints.CancelAllOrdersUriPath)
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
    /// Requests an account context reload after a trade command, and additionally an open orders reload if the
    /// command failed, since a failure may still have changed order state on the exchange.
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
    /// Publishes the reloaded account context as full-snapshot <c>Init</c> events.
    /// </summary>
    /// <remarks>
    /// The positions snapshot is published too, and is always empty. That is a statement rather than a gap -
    /// a spot account cannot hold a position - and publishing it keeps a consumer reading both collections
    /// from waiting forever on one that is never written.
    /// </remarks>
    /// <param name="context">The reloaded account context.</param>
    private void HandleContext(UserContext context)
    {
        Write(ChangeEvent.Init(context.Assets));
        Write(ChangeEvent.Init(context.Positions));
    }

    /// <summary>
    /// Publishes the reloaded open orders as a full-snapshot <c>Init</c> event, with any order the stream has
    /// already reported as over left out of it.
    /// </summary>
    /// <remarks>
    /// A snapshot describes the account as of the moment its request was sent, not the moment its answer
    /// arrived, so a request sent just before an order ended comes back still listing it. Published as it
    /// arrives, that raises an order the caller has already been told was cancelled - and the order is gone
    /// from every later snapshot, so nothing ever corrects it.
    /// </remarks>
    /// <param name="orders">The reloaded open orders.</param>
    private void HandleOrders(IReadOnlyCollection<OrderModel> orders) =>
        Write(ChangeEvent.Init(WithoutOrdersAlreadyOver(orders)));

    /// <summary>
    /// Publishes each reloaded trade individually.
    /// </summary>
    /// <param name="symbol">The symbol trades were reloaded for.</param>
    /// <param name="since">The timestamp trades were reloaded since.</param>
    /// <param name="items">The reloaded trades.</param>
    private void HandleTrades(string symbol, long since, IReadOnlyCollection<TradeModel> items)
    {
        foreach (var item in items)
            Write(item);
    }

    /// <summary>Starts the context and orders loaders once the account stream is subscribed.</summary>
    private void HandleConnected()
    {
        this.Trace<string>("{id} start", Id);

        _contextLoader.Start(true);
        _ordersLoader.Start(true);

        this.Trace<string>("{id} done", Id);
    }

    /// <summary>Stops the context and orders loaders once the account stream drops.</summary>
    private void HandleDisconnected()
    {
        this.Trace<string>("{id} start", Id);

        _contextLoader.Stop();
        _ordersLoader.Stop();

        this.Trace<string>("{id} done", Id);
    }

    /// <summary>
    /// Handles one account event: an order update is published, a balance update triggers a context reload.
    /// </summary>
    /// <remarks>
    /// The balance event is not published from directly, even though it carries the balances it changed. It
    /// carries only <b>those</b>, where the connector publishes the account's assets as a full snapshot, so
    /// writing it straight through would replace the account with the fragment that happened to move.
    /// </remarks>
    /// <param name="data">The raw event payload, already unwrapped from its transport envelope.</param>
    private void HandleMessage(ReadOnlyMemory<byte> data)
    {
        // guarded: decoding the payload to a string is the whole message, and arguments are evaluated
        // before the level is looked at - this runs on every message the account stream delivers
        if (LogConfig.IsEnabled(LogLevel.Trace))
            this.Trace<string, string>("{id} handle {msg}", Id, Encoding.UTF8.GetString(data.Span));

        var orderUpdate = _orderUpdateEventSerializer.Deserialize<OrderUpdateEvent?>(data);
        if (orderUpdate is not null)
        {
            HandleOrderUpdate(orderUpdate);
            return;
        }

        var accountUpdate = _accountUpdateEventSerializer.Deserialize<AccountUpdateEvent?>(data);
        if (accountUpdate is not null)
            _contextLoader.Request();
    }

    /// <summary>
    /// Publishes an order update as a <c>Set</c> while the order is still open and a <c>Delete</c> once it is
    /// not, and requests a trades reload on a fill.
    /// </summary>
    /// <remarks>
    /// The reload is what publishes the trade, even though this event carries the fill's price, quantity and
    /// commission. One source for a trade rather than two: published from both, every fill would reach a
    /// caller twice, and the deduplication that would then be needed is a worse thing to get wrong than a
    /// reload is to wait for.
    /// </remarks>
    /// <param name="e">The order update event.</param>
    private void HandleOrderUpdate(OrderUpdateEvent e)
    {
        if (e.Status is OrderStatus.PartiallyFilled or OrderStatus.Filled)
            _tradesLoader.Request(e.Symbol);

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
            false,
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

    /// <summary>
    /// Removes from a snapshot the orders a stream event has already reported as over, and forgets the ones
    /// the snapshot itself no longer lists.
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
            // The cap is for the case where snapshots stop arriving while the stream keeps going - a reload
            // failing against a rate limit does exactly that, for minutes - and nothing would retire
            // anything. A note that old protects nothing anyway: it guards against a snapshot already in
            // flight, and once the loader recovers, the snapshot it sends lists none of them.
            while (_endedOrder.Count > MaxEndedNotes)
                _ended.Remove(_endedOrder.Dequeue());
        }
    }

    /// <summary>
    /// Says whether an order has already been reported as over.
    /// </summary>
    /// <remarks>
    /// A stale snapshot is not the only way an order comes back to life: the venue's own events are not
    /// ordered against each other either, so a placement event can land after the cancellation of the same
    /// order. Over is final here and no id is reused, so the report that came first in time is the one that
    /// stands, whichever arrived last.
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
}
