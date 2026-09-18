using System;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Data.Tables;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Net.Servers.Web;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User;

/// <summary>
/// Pins what the USD-M futures connector does with what arrives: a user-stream message, a load from one of
/// its loaders, and the stream going up and down.
/// </summary>
/// <remarks>
/// The live block observes that data arrives. What it cannot do is choose the message - a fill, a
/// cancellation, a shape the exchange sends rarely - and the handling of each is a separate branch.
/// </remarks>
public class UserConnectorIngestionTests : UserConnectorOfflineTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectorIngestionTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public UserConnectorIngestionTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Every message reloads the account context, whatever the message was.
    /// </summary>
    /// <remarks>
    /// Deliberate: the account update event carries positions without leverage, so the connector re-reads
    /// rather than trusting the event. A message it does not even recognise is no exception - something
    /// happened on the account.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task AnyMessage_ReloadsTheContext()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);

        // act
        parts.Stream.Push(Encoding.UTF8.GetBytes("""{"e":"SOMETHING_ELSE"}"""));

        // assert
        await parts.ContextLoader.Requests.Reader.ReadAsync(ct);
    }

    /// <summary>
    /// A fill asks for the symbol's trades, because the event does not carry what they are worth.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Fill_RequestsTheSymbolsTrades()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);

        // act
        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("FILLED", "1")));

        // assert
        (await parts.TradesLoader.Requests.Reader.ReadAsync(ct)).Is("BTCUSDT");
    }

    /// <summary>
    /// An order that is merely placed does not ask for trades - there are none yet.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task New_DoesNotRequestTrades()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);

        // act
        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("NEW", "0")));

        // assert
        // the context reload is the message's other effect, and it is the one that tells us the message
        // was processed at all - so the absence below is an absence after the fact, not before it
        await parts.ContextLoader.Requests.Reader.ReadAsync(ct);
        parts.TradesLoader.Requests.Reader.Count.Is(0);
    }

    /// <summary>
    /// An order still in play is written as a change; an order that is over is written as a deletion.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task OrderUpdate_LivingOrderIsSetAndFinishedOrderIsDeleted()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var orders = Channel.CreateUnbounded<ChangeEvent<OrderModel>>();
        using var subscription = connector.Orders.Subscribe(x => orders.Writer.TryWrite(x));

        // writes are buffered until a sync cycle completes, so nothing reaches a subscriber before this
        connector.Sync();

        // act
        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("NEW", "0")));
        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("CANCELED", "0")));

        // assert
        var placed = await ReadOrderAsync(orders, ct);
        placed.Type.Is(ChangeEventType.Set);
        placed.Item.Symbol.Is("BTCUSDT");
        placed.Item.Status.Is(OrderStatus.New);

        var gone = await ReadOrderAsync(orders, ct);
        gone.Type.Is(ChangeEventType.Delete);
        gone.Item.Status.Is(OrderStatus.Canceled);
    }

    /// <summary>
    /// The stream coming up starts the loaders, and going down stops them.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task StreamLifecycle_StartsAndStopsTheLoaders()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var startsBefore = parts.ContextLoader.Starts;

        // act
        parts.Stream.Connect();

        // assert
        parts.ContextLoader.Starts.Is(startsBefore + 1);
        parts.OrdersLoader.Starts.Is(startsBefore + 1);

        parts.Stream.Disconnect();
        parts.ContextLoader.Stops.Is(1);
        parts.OrdersLoader.Stops.Is(1);

        // nothing above waits on the exchange; the token is here so a hang ends with the test rather
        // than with the runner
        await Task.Delay(1, ct);
    }

    /// <summary>
    /// Reads the next order change a subscriber saw.
    /// </summary>
    /// <param name="orders">The channel the subscriber writes into.</param>
    /// <param name="ct">The test's cancellation token.</param>
    /// <returns>The next order change.</returns>
    private static async Task<ChangeEvent<OrderModel>> ReadOrderAsync(
        Channel<ChangeEvent<OrderModel>> orders,
        CancellationToken ct
    ) => await orders.Reader.ReadAsync(ct);

    /// <summary>
    /// Builds a Binance order update event.
    /// </summary>
    /// <param name="status">The order status the event reports.</param>
    /// <param name="executedQty">The cumulative filled quantity the event reports.</param>
    /// <returns>The event payload.</returns>
    private static string OrderUpdate(string status, string executedQty) =>
        $$$"""
            {"e":"ORDER_TRADE_UPDATE","E":1700000000000,"T":1700000000000,"o":{
            "s":"BTCUSDT","c":"2f1d4e6a-8b3c-4d5e-9f01-23456789abcd","S":"BUY","o":"LIMIT","f":"GTC",
            "q":"1","p":"100","ap":"0","sp":"0","x":"NEW","X":"{{{status}}}","i":1,"l":"0","z":"{{{executedQty}}}",
            "L":"0","T":1700000000000,"t":0,"b":"0","a":"0","m":false,"R":false,"wt":"CONTRACT_PRICE",
            "ot":"LIMIT","ps":"BOTH","cp":false,"rp":"0","pP":false}}
            """;

    /// <summary>
    /// Starts a local server that answers anything with an empty object; the ingestion paths do not call it.
    /// </summary>
    /// <returns>The running server; dispose it to stop listening.</returns>
    private IServer RunServer() =>
        this.RunHttpServer(
            async (_, response) =>
            {
                var payload = Encoding.UTF8.GetBytes("{}");
                response.StatusCode(HttpStatusCode.OK);
                response.ContentType = MediaTypeNames.Application.Json;
                response.ContentLength64 = payload.Length;
                await response.OutputStream.WriteAsync(payload);
            }
        );
}
