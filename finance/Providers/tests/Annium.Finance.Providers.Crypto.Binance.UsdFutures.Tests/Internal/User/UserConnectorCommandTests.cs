using System;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Abstractions.Domain.User.Operations;
using Annium.Finance.Providers.Abstractions.Domain.User.Requests;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User;

/// <summary>
/// Pins the command paths of the USD-M futures user connector against a local server: what it refuses to
/// send, what it sends, and what it reloads afterwards.
/// </summary>
/// <remarks>
/// The live block drives the happy path of four commands. Everything here is what that block structurally
/// cannot reach - a disconnected connector, a query that does not build, a modify of an order that is not a
/// limit - plus the one command it never calls at all.
/// </remarks>
public class UserConnectorCommandTests : UserConnectorOfflineTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectorCommandTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public UserConnectorCommandTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A disconnected connector refuses every command without reaching the exchange.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task NotConnected_EveryCommandRefusesWithoutSending()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, "{}"));

        // a second target the monitor never sees connected, so the aggregate - and the connector's own
        // view of itself - is not Connected
        var monitor = new StatusMonitor(Get<ILogger>());
        var peer = monitor.CreateReporter();
        peer.Bind(new object(), ConnectorStatus.Connecting);

        await using var connector = CreateConnector(server, out var parts, monitor);

        // act
        var leverage = await connector.SetLeverageAsync(Position(), 5m);
        var init = await connector.InitOrderAsync(LimitOrderRequest());
        var modify = await connector.ModifyOrderAsync(ModifyToLimitRequest());
        var cancel = await connector.CancelOrderAsync(CancelRequest("1"));
        var cancelAll = await connector.CancelAllOrdersAsync("BTCUSDT");

        // assert
        leverage.Status.Is(UserOperationStatus.NotConnected);
        init.Status.Is(UserOperationStatus.NotConnected);
        modify.Status.Is(UserOperationStatus.NotConnected);
        cancel.Status.Is(UserOperationStatus.NotConnected);
        cancelAll.Status.Is(UserOperationStatus.NotConnected);

        // absence needs a window: a request wrongly issued would still be in flight at this point
        await Task.Delay(50, ct);
        log.Requests.Count.Is(0);
        parts.ContextLoader.Requests.Reader.Count.Is(0);
        parts.OrdersLoader.Requests.Reader.Count.Is(0);
    }

    /// <summary>
    /// Setting leverage floors it to a whole number, as the exchange requires.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task SetLeverage_FloorsToWholeNumber()
    {
        // arrange
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, """{"leverage":5,"symbol":"BTCUSDT"}"""));
        await using var connector = CreateConnector(server, out _);

        // act
        await connector.SetLeverageAsync(Position(), 5.9m);

        // assert
        var request = await log.Answered.Reader.ReadAsync(TestContext.Current.CancellationToken);
        request.Method.Is("POST");
        request.Path.Is("/fapi/v1/leverage");
        request.Query.Contains("leverage=5").IsTrue();
    }

    /// <summary>
    /// A refused leverage change reaches the caller.
    /// </summary>
    /// <remarks>
    /// It used to report Ok whatever came back, which left a caller sizing positions against a leverage
    /// the account does not have.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task SetLeverage_RefusalReachesTheCaller()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.BadRequest, """{"code":-4028,"msg":"bad"}"""));
        await using var connector = CreateConnector(server, out var parts);

        // act
        var result = await connector.SetLeverageAsync(Position(), 5m);

        // assert
        result.Status.Is(UserOperationStatus.BadRequest);

        // and the account is re-read either way, which is how the connector finds out what it looks like now
        await parts.ContextLoader.Requests.Reader.ReadAsync(ct);
        await parts.OrdersLoader.Requests.Reader.ReadAsync(ct);
    }

    /// <summary>
    /// A leverage change the exchange accepts without applying is refused by the connector itself.
    /// </summary>
    /// <remarks>
    /// The response carries the leverage the account ended up with, and that is the only place the
    /// difference shows: to a caller reading the status alone, "accepted and ignored" looks exactly like
    /// success. The synthetic refusal is what makes the two distinguishable.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task SetLeverage_AcceptedButNotAppliedIsRefusedByTheConnector()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, """{"leverage":3,"symbol":"BTCUSDT"}"""));
        await using var connector = CreateConnector(server, out var parts);

        // act
        var result = await connector.SetLeverageAsync(Position(), 5m);

        // assert
        result.Status.Is(UserOperationStatus.UnknownError);
        result.Message.Contains("5").IsTrue();
        result.Message.Contains("3").IsTrue();

        // the exchange accepted it, so the reload is the success one: context only
        await parts.ContextLoader.Requests.Reader.ReadAsync(ct);
        parts.OrdersLoader.Requests.Reader.Count.Is(0);
    }

    /// <summary>
    /// A leverage change the account applied is reported as success.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task SetLeverage_AppliedIsOk()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, """{"leverage":5,"symbol":"BTCUSDT"}"""));
        await using var connector = CreateConnector(server, out var parts);

        // act
        // 5.9 floored to 5, which is what the account reports back
        var result = await connector.SetLeverageAsync(Position(), 5.9m);

        // assert
        result.Status.Is(UserOperationStatus.Ok);
        await parts.ContextLoader.Requests.Reader.ReadAsync(ct);
    }

    /// <summary>
    /// A cancel that names neither an id nor a client order id never reaches the exchange.
    /// </summary>
    /// <remarks>
    /// The query processor has tests of its own; what is pinned here is the connector's handling of its
    /// refusal - no request, and no reload either, because nothing about the account changed.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Cancel_QueryThatDoesNotBuildIsNotSent()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, "{}"));
        await using var connector = CreateConnector(server, out var parts);

        // act
        var result = await connector.CancelOrderAsync(CancelRequest(string.Empty));

        // assert
        result.Status.Is(UserOperationStatus.BadRequest);

        // absence needs a window: a request wrongly issued would still be in flight at this point
        await Task.Delay(50, ct);
        log.Requests.Count.Is(0);
        parts.ContextLoader.Requests.Reader.Count.Is(0);
        parts.OrdersLoader.Requests.Reader.Count.Is(0);
    }

    /// <summary>
    /// Modifying an order that is not a limit is a cancel and a re-init, not an amend.
    /// </summary>
    /// <remarks>
    /// The exchange amends limit orders only. The live tests only ever modify limits, so this branch - the
    /// one that touches the account twice - is reachable nowhere else.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Modify_NonLimitOrderIsCancelledAndPlacedAnew()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(
            log,
            request =>
                request.Method == "DELETE" ? (HttpStatusCode.OK, CancelResponse) : (HttpStatusCode.OK, OrderResponse)
        );
        await using var connector = CreateConnector(server, out _);

        // act
        await connector.ModifyOrderAsync(ModifyNonLimitRequest());

        // assert
        var cancel = await log.Answered.Reader.ReadAsync(ct);
        cancel.Method.Is("DELETE");
        cancel.Path.Is("/fapi/v1/order");

        var init = await log.Answered.Reader.ReadAsync(ct);
        init.Method.Is("POST");
        init.Path.Is("/fapi/v1/order");

        // and never the amend endpoint, which is what a limit would have taken
        log.Requests.Count.Is(2);
    }

    /// <summary>
    /// Turning a limit order into a market order is refused before anything is sent.
    /// </summary>
    /// <remarks>
    /// The cancel-and-reinit path keys on the type of the order that <em>exists</em>, while the query
    /// builder keys on the type being asked for - so a limit order asked to become a market order takes
    /// neither route and is refused with "Only limit orders are supported". Decided: the refusal is the
    /// contract. Routing it to cancel-and-reinit would mean a caller asking to amend an order and silently
    /// getting a new one instead - at a new place in the queue, and at whatever the market has moved to.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Modify_LimitOrderIntoMarketOrderIsRefused()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, OrderResponse));
        await using var connector = CreateConnector(server, out _);

        // act
        var result = await connector.ModifyOrderAsync(LimitToMarketRequest());

        // assert
        result.Status.Is(UserOperationStatus.BadRequest);

        // absence needs a window: a request wrongly issued would still be in flight at this point
        await Task.Delay(50, ct);
        log.Requests.Count.Is(0);
    }

    /// <summary>
    /// A successful command reloads the account context and leaves the orders alone.
    /// </summary>
    /// <remarks>
    /// The asymmetry is deliberate: a command that succeeded already told the connector what happened to
    /// the order, while a command that failed left it guessing, so the orders are re-read as well.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Command_SuccessReloadsContextOnly()
    {
        // arrange
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, OrderResponse));
        await using var connector = CreateConnector(server, out var parts);

        // act
        var result = await connector.InitOrderAsync(LimitOrderRequest());

        // assert
        result.Status.Is(UserOperationStatus.Ok);
        await parts.ContextLoader.Requests.Reader.ReadAsync(TestContext.Current.CancellationToken);
        parts.OrdersLoader.Requests.Reader.Count.Is(0);
    }

    /// <summary>
    /// A refused command reloads the orders too, since the connector no longer knows what stands.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Command_FailureReloadsOrdersAsWell()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.BadRequest, """{"code":-2010,"msg":"refused"}"""));
        await using var connector = CreateConnector(server, out var parts);

        // act
        var result = await connector.InitOrderAsync(LimitOrderRequest());

        // assert
        result.Status.Is(UserOperationStatus.BadRequest);
        await parts.ContextLoader.Requests.Reader.ReadAsync(ct);
        await parts.OrdersLoader.Requests.Reader.ReadAsync(ct);
    }

    /// <summary>
    /// A canned successful cancel response, in the shape the exchange sends one.
    /// </summary>
    /// <remarks>
    /// The client order id has to be a GUID: the cancel response converter reads it as one and returns no
    /// response at all when it is not, which reads back as a failed cancel.
    /// </remarks>
    private const string CancelResponse = """
        {"orderId":1,"clientOrderId":"2f1d4e6a-8b3c-4d5e-9f01-23456789abcd","symbol":"BTCUSDT","status":"CANCELED"}
        """;

    /// <summary>A real placement answer from the algo endpoint, captured live on 2026-09-19.</summary>
    private const string AlgoOrderResponse = """
        {
          "algoId": 4000001910058351,
          "clientAlgoId": "order-1",
          "algoType": "CONDITIONAL",
          "orderType": "STOP_MARKET",
          "symbol": "BTCUSDT",
          "side": "SELL",
          "positionSide": "BOTH",
          "quantity": "1",
          "algoStatus": "NEW",
          "triggerPrice": "90",
          "price": "0",
          "reduceOnly": false,
          "createTime": 1789819033056,
          "updateTime": 1789819033056
        }
        """;

    /// <summary>
    /// A canned successful order response, in the shape the exchange sends one.
    /// </summary>
    private const string OrderResponse = """
        {"orderId":1,"clientOrderId":"c1","symbol":"BTCUSDT","status":"NEW","side":"BUY","type":"LIMIT",
        "positionSide":"BOTH","origQty":"1","price":"100","stopPrice":"0","reduceOnly":false,
        "executedQty":"0","avgPrice":"0","updateTime":1700000000000,"timeInForce":"GTC"}
        """;

    /// <summary>
    /// Starts a local server that answers every request from the given script and records what it was asked.
    /// </summary>
    /// <param name="log">The log to record requests into.</param>
    /// <param name="respond">Builds the response for a recorded request.</param>
    /// <returns>The running server; dispose it to stop listening.</returns>
    private IServer RunServer(RequestLog log, Func<RecordedRequest, (HttpStatusCode Code, string Body)> respond) =>
        this.RunHttpServer(
            async (request, response) =>
            {
                var recorded = new RecordedRequest(
                    request.HttpMethod,
                    request.Url?.AbsolutePath ?? string.Empty,
                    request.Url?.Query.TrimStart('?') ?? string.Empty
                );

                var (code, body) = respond(recorded);
                var payload = Encoding.UTF8.GetBytes(body);

                response.StatusCode(code);
                response.ContentType = MediaTypeNames.Application.Json;
                response.ContentLength64 = payload.Length;
                await response.OutputStream.WriteAsync(payload);

                log.Add(recorded.Method, recorded.Path, recorded.Query);
            }
        );

    /// <summary>
    /// A position to change leverage for.
    /// </summary>
    /// <returns>The position.</returns>
    private static PositionModel Position() => new("BTCUSDT", OrientationRange.Both, MarginType.Cross, 1m, 0m);

    /// <summary>
    /// A conditional order goes to the algo endpoint, under the algo parameter names.
    /// </summary>
    /// <remarks>
    /// The four conditional types were refused on the ordinary endpoint from 2025-12-09 with <c>-4120</c>,
    /// which is how this was discovered - by a live order being turned down rather than by anything in the
    /// tree. The path is the assertion, and the parameter names with it: the client id is
    /// <c>clientAlgoId</c>, the trigger is <c>triggerPrice</c>, and <c>algoType</c> is required and has no
    /// counterpart on the ordinary endpoint at all.
    /// </remarks>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task InitOrder_Conditional_GoesToTheAlgoEndpoint()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, AlgoOrderResponse));
        await using var connector = CreateConnector(server, out _);

        // act
        await connector.InitOrderAsync(StopLossMarketOrderRequest());

        // assert
        var sent = await log.Answered.Reader.ReadAsync(ct);
        sent.Method.Is("POST");
        sent.Path.Is("/fapi/v1/algoOrder", "a conditional order went to the endpoint that refuses it");
        sent.Query.Contains("algoType=CONDITIONAL").IsTrue($"algoType was not sent: {sent.Query}");
        sent.Query.Contains("clientAlgoId=order-1").IsTrue($"the client id was not sent as clientAlgoId: {sent.Query}");
        sent.Query.Contains("triggerPrice=").IsTrue($"the trigger was not sent as triggerPrice: {sent.Query}");
        sent.Query.Contains("stopPrice=").IsFalse($"the ordinary endpoint's stopPrice was sent: {sent.Query}");
        sent.Query.Contains("newClientOrderId=")
            .IsFalse($"the ordinary endpoint's newClientOrderId was sent: {sent.Query}");
    }

    /// <summary>
    /// A market order still goes to the ordinary endpoint, which is the half of the routing that would
    /// break silently.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task InitOrder_NonConditional_StaysOnTheOrdinaryEndpoint()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, OrderResponse));
        await using var connector = CreateConnector(server, out _);

        // act
        await connector.InitOrderAsync(LimitOrderRequest());

        // assert
        var sent = await log.Answered.Reader.ReadAsync(ct);
        sent.Path.Is("/fapi/v1/order", "an ordinary order was routed to the conditional endpoint");
    }

    /// <summary>
    /// A conditional order request.
    /// </summary>
    /// <returns>The request.</returns>
    private static IInitOrderRequest StopLossMarketOrderRequest() =>
        RequestBuilder.InitStopLossMarketOrder("order-1", OrientationRange.Both, "BTCUSDT", OrderSide.Sell, 1m, 90m);

    /// <summary>
    /// A limit order request.
    /// </summary>
    /// <returns>The request.</returns>
    private static IInitOrderRequest LimitOrderRequest() =>
        RequestBuilder.InitLimitOrder("order-1", OrientationRange.Both, "BTCUSDT", OrderSide.Buy, 1m, 100m);

    /// <summary>
    /// A request modifying an order into a limit order.
    /// </summary>
    /// <returns>The request.</returns>
    private static IModifyOrderRequest ModifyToLimitRequest() =>
        RequestBuilder.ModifyToLimitOrder(Order(OrderType.Limit), OrderSide.Buy, 1m, 100m);

    /// <summary>
    /// A request modifying an order that is not a limit, which the exchange cannot amend at all.
    /// </summary>
    /// <returns>The request.</returns>
    private static IModifyOrderRequest ModifyNonLimitRequest() =>
        RequestBuilder.ModifyToLimitOrder(Order(OrderType.StopLossMarket), OrderSide.Buy, 1m, 100m);

    /// <summary>
    /// A request turning a limit order into a market order.
    /// </summary>
    /// <returns>The request.</returns>
    private static IModifyOrderRequest LimitToMarketRequest() =>
        RequestBuilder.ModifyToMarketOrder(Order(OrderType.Limit), OrderSide.Buy, 1m);

    /// <summary>
    /// A cancel request naming the given id.
    /// </summary>
    /// <param name="id">The order id to cancel; empty for a request that cannot build a query.</param>
    /// <returns>The request.</returns>
    private static ICancelOrderRequest CancelRequest(string id) =>
        RequestBuilder.CancelOrder(id, string.Empty, "BTCUSDT");

    /// <summary>
    /// An order to modify.
    /// </summary>
    /// <param name="type">The order's type.</param>
    /// <returns>The order.</returns>
    private static OrderModel Order(OrderType type) =>
        new(
            "1",
            "c1",
            OrientationRange.Both,
            "BTCUSDT",
            OrderSide.Buy,
            type,
            1m,
            100m,
            0m,
            false,
            1_700_000_000_000,
            OrderStatus.New,
            0m,
            0m,
            1_700_000_000_000
        );
}
