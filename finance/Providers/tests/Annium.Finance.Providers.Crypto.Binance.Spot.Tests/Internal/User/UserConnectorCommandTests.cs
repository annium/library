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

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User;

/// <summary>
/// Pins the command paths of the Binance spot user connector against a local server: what it refuses to
/// send, where it sends what it does send, and what it reloads afterwards.
/// </summary>
/// <remarks>
/// There is no live trading block for this venue, so the endpoint each command goes to is pinned here or
/// nowhere. That is the assertion worth making offline: a local server answers whatever it is asked, so a
/// wrong path is invisible in every other test and surfaces only against the exchange - which is how this
/// module has already lost a live run once.
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
        var init = await connector.InitOrderAsync(LimitOrderRequest());
        var modify = await connector.ModifyOrderAsync(ModifyToLimitRequest());
        var cancel = await connector.CancelOrderAsync(CancelRequest("1"));
        var cancelAll = await connector.CancelAllOrdersAsync("BTCUSDT");

        // assert
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
    /// Each command goes to the endpoint and method the contract names for it.
    /// </summary>
    /// <remarks>
    /// The replace endpoint is the one to look at. It is a <c>POST</c> to a cancel-and-replace path rather
    /// than a <c>PUT</c> to an amend path, because this venue's amend reduces quantity and nothing else -
    /// so an implementation ported from a venue that amends properly would compile, pass every test that
    /// only checks outcomes, and be refused for a parameter it is not allowed to send.
    /// </remarks>
    /// <param name="command">The command to drive.</param>
    /// <param name="method">The HTTP method it must use.</param>
    /// <param name="path">The path it must go to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory(Timeout = Timeout)]
    [InlineData("init", "POST", "/api/v3/order")]
    [InlineData("modify", "POST", "/api/v3/order/cancelReplace")]
    [InlineData("cancel", "DELETE", "/api/v3/order")]
    [InlineData("cancelAll", "DELETE", "/api/v3/openOrders")]
    public async Task EachCommand_GoesToItsDocumentedEndpoint(string command, string method, string path)
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, OrderResponse));
        await using var connector = CreateConnector(server, out _);

        // act
        switch (command)
        {
            case "init":
                await connector.InitOrderAsync(LimitOrderRequest());
                break;
            case "modify":
                await connector.ModifyOrderAsync(ModifyToLimitRequest());
                break;
            case "cancel":
                await connector.CancelOrderAsync(CancelRequest("1"));
                break;
            default:
                await connector.CancelAllOrdersAsync("BTCUSDT");
                break;
        }

        // assert
        var request = await log.Answered.Reader.ReadAsync(ct);
        request.Method.Is(method, $"{command} used the wrong method");
        request.Path.Is(path, $"{command} went to the wrong endpoint");
    }

    /// <summary>
    /// A replacement is sent as one cancel-and-replace request that stops if the cancel fails.
    /// </summary>
    /// <remarks>
    /// Sent as a cancel followed by a placement, a cancel that succeeds and a placement that does not
    /// leaves the account with neither order and the caller with a failure that does not say so. The mode
    /// is what makes the pair atomic, which is why it is asserted rather than assumed from the endpoint.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Modify_StopsOnAFailedCancelRatherThanReplacingRegardless()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, OrderResponse));
        await using var connector = CreateConnector(server, out _);

        // act
        await connector.ModifyOrderAsync(ModifyToLimitRequest());

        // assert
        var request = await log.Answered.Reader.ReadAsync(ct);
        request.Query.Contains("cancelReplaceMode=STOP_ON_FAILURE").IsTrue(request.Query);

        // one request, not a cancel and a placement
        await Task.Delay(50, ct);
        log.Requests.Count.Is(1);
    }

    /// <summary>
    /// Setting leverage is refused, because a spot account holds no leveraged positions.
    /// </summary>
    /// <remarks>
    /// The alternative that looks harmless is reporting success, and it is the worse of the two: a caller
    /// told its leverage was applied sizes the next position against a number the account has never heard
    /// of. Nothing is sent, so there is no endpoint to get wrong either.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task SetLeverage_IsRefusedAndSendsNothing()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, "{}"));
        await using var connector = CreateConnector(server, out _);

        // act
        var result = await connector.SetLeverageAsync(Position(), 5m);

        // assert
        result.Status.IsNot(UserOperationStatus.Ok);
        result.Status.Is(UserOperationStatus.BadRequest);

        await Task.Delay(50, ct);
        log.Requests.Count.Is(0);
    }

    /// <summary>
    /// A cancellation naming no order at all is refused before anything is sent.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Cancel_QueryThatDoesNotBuildIsNotSent()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, "{}"));
        await using var connector = CreateConnector(server, out _);

        // act
        var result = await connector.CancelOrderAsync(CancelRequest(string.Empty));

        // assert
        result.Status.Is(UserOperationStatus.BadRequest);

        await Task.Delay(50, ct);
        log.Requests.Count.Is(0);
    }

    /// <summary>
    /// A command that succeeds reloads the account context and nothing else.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Command_SuccessReloadsContextOnly()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, OrderResponse));
        await using var connector = CreateConnector(server, out var parts);

        // act
        var result = await connector.InitOrderAsync(LimitOrderRequest());

        // assert
        result.Status.Is(UserOperationStatus.Ok);
        (await parts.ContextLoader.Requests.Reader.ReadAsync(ct)).Is(0);
        parts.OrdersLoader.Requests.Reader.Count.Is(0);
    }

    /// <summary>
    /// A command that fails reloads the open orders as well.
    /// </summary>
    /// <remarks>
    /// A refused command may still have changed order state - a replacement whose cancel succeeded and
    /// whose placement did not is exactly that - so a failure is the moment the connector knows least
    /// about the account and has to go and look.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Command_FailureReloadsOrdersAsWell()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(
            log,
            _ => (HttpStatusCode.BadRequest, """{"code":-2010,"msg":"Account has insufficient balance."}""")
        );
        await using var connector = CreateConnector(server, out var parts);

        // act
        var result = await connector.InitOrderAsync(LimitOrderRequest());

        // assert
        result.Status.IsNot(UserOperationStatus.Ok);
        (await parts.ContextLoader.Requests.Reader.ReadAsync(ct)).Is(0);
        (await parts.OrdersLoader.Requests.Reader.ReadAsync(ct)).Is(0);
    }

    /// <summary>
    /// An exchange refusal reaches the caller as a failure rather than as a placed order.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task ARefusedPlacement_IsAFailureAndNotAnOrder()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(
            log,
            _ => (HttpStatusCode.BadRequest, """{"code":-1013,"msg":"Filter failure: MIN_NOTIONAL"}""")
        );
        await using var connector = CreateConnector(server, out _);

        // act
        var result = await connector.InitOrderAsync(LimitOrderRequest());

        // assert
        result.Status.IsNot(UserOperationStatus.Ok, $"a refusal was reported as {result.Status}");
        result.Data.IsDefault();

        // the refusal was the server's, not a request that never went out
        (await log.Answered.Reader.ReadAsync(ct)).Path.Is("/api/v3/order");
    }

    /// <summary>
    /// Cancelling all orders is reported as a success when the venue answers with what it cancelled.
    /// </summary>
    /// <remarks>
    /// The fixture below is the venue's real answer, recorded off the wire on 2026-09-21. It matters
    /// because the other venue answers a cancel-all with an acknowledgement envelope and this one answers
    /// the <b>list of orders it cancelled</b>: read as the envelope, every successful cancellation came
    /// back to the caller as a failure to parse. The cancellation had already happened on the exchange,
    /// which is the half that makes it worth a test rather than a fix - a caller told it failed either
    /// retries into a refusal or believes orders are still open.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task CancelAll_ReadsTheListOfCancelledOrders()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var log = new RequestLog();
        await using var server = RunServer(log, _ => (HttpStatusCode.OK, CancelAllResponse));
        await using var connector = CreateConnector(server, out _);

        // act
        var result = await connector.CancelAllOrdersAsync("DOTUSDT");

        // assert
        result.Status.Is(UserOperationStatus.Ok, $"the venue's own answer was read as {result.Status}");
        (await log.Answered.Reader.ReadAsync(ct)).Path.Is("/api/v3/openOrders");
    }

    /// <summary>
    /// The venue's answer to a cancel-all, recorded off the wire on 2026-09-21.
    /// </summary>
    private const string CancelAllResponse = """
        [
            {
                "symbol": "DOTUSDT",
                "origClientOrderId": "87f263d8-0c6e-4aec-9f4f-80eea9996601",
                "orderId": 6205116330,
                "orderListId": -1,
                "clientOrderId": "oZxIAGe3LR6MnCw26VGpgk",
                "transactTime": 1789975849417,
                "price": "0.68900000",
                "origQty": "7.98000000",
                "executedQty": "0.00000000",
                "origQuoteOrderQty": "0.00000000",
                "cummulativeQuoteQty": "0.00000000",
                "status": "CANCELED",
                "timeInForce": "GTC",
                "type": "LIMIT",
                "side": "BUY",
                "selfTradePreventionMode": "EXPIRE_MAKER"
            }
        ]
        """;

    /// <summary>
    /// A recorded answer to a placement, used wherever a command needs to succeed.
    /// </summary>
    private const string OrderResponse = """
        {
            "symbol": "BTCUSDT",
            "orderId": 28,
            "orderListId": -1,
            "clientOrderId": "order-1",
            "price": "100.00",
            "origQty": "1.00",
            "executedQty": "0.00",
            "cummulativeQuoteQty": "0.00",
            "status": "NEW",
            "timeInForce": "GTC",
            "type": "LIMIT",
            "side": "BUY",
            "stopPrice": "0.00",
            "time": 1499827319559,
            "updateTime": 1499827319559,
            "isWorking": true,
            "workingTime": 1499827319559,
            "selfTradePreventionMode": "NONE"
        }
        """;

    /// <summary>
    /// Runs a local server that records every request and answers as the test says.
    /// </summary>
    /// <param name="log">The log to record requests into.</param>
    /// <param name="respond">Decides the answer for a request.</param>
    /// <returns>The running server.</returns>
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
    /// A position to ask for a leverage change on.
    /// </summary>
    /// <returns>The position.</returns>
    private static PositionModel Position() => new("BTCUSDT", OrientationRange.Both, MarginType.Cross, 1m, 0m);

    /// <summary>
    /// A limit order request.
    /// </summary>
    /// <returns>The request.</returns>
    private static IInitOrderRequest LimitOrderRequest() =>
        RequestBuilder.InitLimitOrder("order-1", OrientationRange.Both, "BTCUSDT", OrderSide.Buy, 1m, 100m);

    /// <summary>
    /// A request replacing an order with a limit order.
    /// </summary>
    /// <returns>The request.</returns>
    private static IModifyOrderRequest ModifyToLimitRequest() =>
        RequestBuilder.ModifyToLimitOrder(Order(OrderType.Limit), OrderSide.Buy, 1m, 100m);

    /// <summary>
    /// A cancel request naming the given id.
    /// </summary>
    /// <param name="id">The order id to cancel; empty for a request that cannot build a query.</param>
    /// <returns>The request.</returns>
    private static ICancelOrderRequest CancelRequest(string id) =>
        RequestBuilder.CancelOrder(id, string.Empty, "BTCUSDT", OrderType.Limit);

    /// <summary>
    /// An order to replace.
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
