using System;
using System.Linq;
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
    /// A snapshot that still lists an order the stream has already ended does not raise it again.
    /// </summary>
    /// <remarks>
    /// A snapshot describes the account as it was when its request was sent, and a request sent a
    /// fraction of a second before an order ends comes back still listing it. Published as it arrives,
    /// it tells the caller an order it has just been told was cancelled is open - and since the order is
    /// gone from every later snapshot, nothing ever corrects it.
    ///
    /// It was found as one live run in four failing on a conditional order that stayed New after being
    /// cancelled, and on the wire the whole window was about 270ms: the cancel left, a reload's request
    /// followed it, the venue's cancellation event arrived, and only then did the reload answer.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Snapshot_DoesNotRaiseAnOrderTheStreamAlreadyEnded()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var orders = Channel.CreateUnbounded<ChangeEvent<OrderModel>>();
        using var subscription = connector.Orders.Subscribe(x => orders.Writer.TryWrite(x));
        connector.Sync();

        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("CANCELED", "0")));
        (await ReadOrderAsync(orders, ct)).Type.Is(ChangeEventType.Delete);

        // act - the answer to a reload that was already in flight, still listing the order as open
        parts.OrdersLoader.Emit([OpenOrder("1"), OpenOrder("2")]);

        // assert
        var snapshot = await ReadOrderAsync(orders, ct);
        snapshot.Type.Is(ChangeEventType.Init);
        snapshot.Items.Count.Is(1, "a cancelled order was raised again by a snapshot older than the cancellation");
        snapshot.Items.Single().Id.Is("2", "the wrong order was dropped from the snapshot");
    }

    /// <summary>
    /// The same holds for a conditional order, which ends on an event of its own.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Snapshot_DoesNotRaiseAConditionalOrderTheStreamAlreadyEnded()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var orders = Channel.CreateUnbounded<ChangeEvent<OrderModel>>();
        using var subscription = connector.Orders.Subscribe(x => orders.Writer.TryWrite(x));
        connector.Sync();

        parts.Stream.Push(Encoding.UTF8.GetBytes(AlgoUpdate("CANCELED")));
        (await ReadOrderAsync(orders, ct)).Type.Is(ChangeEventType.Delete);

        // act
        parts.OrdersLoader.Emit([OpenOrder("4000001912557457")]);

        // assert
        var snapshot = await ReadOrderAsync(orders, ct);
        snapshot.Type.Is(ChangeEventType.Init);
        snapshot.Items.IsEmpty("a cancelled conditional order was raised again by a stale snapshot");
    }

    /// <summary>
    /// A placement event that arrives after the cancellation of the same order does not raise it.
    /// </summary>
    /// <remarks>
    /// The venue's events are not ordered against each other. A placement event was measured arriving
    /// 600ms after the order was placed, which is long enough to land after the cancellation of that
    /// same order when the two are a second apart - and a caller told an order is cancelled and then
    /// told it is new has been misled by the source it trusts most.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task LateLiveEvent_DoesNotRaiseAnOrderAlreadyEnded()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var orders = Channel.CreateUnbounded<ChangeEvent<OrderModel>>();
        using var subscription = connector.Orders.Subscribe(x => orders.Writer.TryWrite(x));
        connector.Sync();

        parts.Stream.Push(Encoding.UTF8.GetBytes(AlgoUpdate("CANCELED")));
        (await ReadOrderAsync(orders, ct)).Type.Is(ChangeEventType.Delete);

        // act - the placement event for the same order, overtaken by its own cancellation
        parts.Stream.Push(Encoding.UTF8.GetBytes(AlgoUpdate("NEW")));

        // assert - a snapshot is pushed behind it, so this waits for something that must arrive after
        parts.OrdersLoader.Emit([]);
        var next = await ReadOrderAsync(orders, ct);
        next.Type.Is(ChangeEventType.Init, "a cancelled order was raised again by a late placement event");
    }

    /// <summary>
    /// Once a snapshot agrees an order is gone, the note about it is dropped rather than kept forever.
    /// </summary>
    /// <remarks>
    /// The note exists only to outlive the one snapshot that was already in flight. Keeping it would
    /// make the connector carry an id per order it ever saw end, for the life of the process.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task Snapshot_ForgetsAnEndedOrderOnceTheVenueAgrees()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var orders = Channel.CreateUnbounded<ChangeEvent<OrderModel>>();
        using var subscription = connector.Orders.Subscribe(x => orders.Writer.TryWrite(x));
        connector.Sync();

        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("CANCELED", "0")));
        (await ReadOrderAsync(orders, ct)).Type.Is(ChangeEventType.Delete);

        // the venue stops listing it, which is what the note was waiting for
        parts.OrdersLoader.Emit([]);
        (await ReadOrderAsync(orders, ct)).Items.IsEmpty();

        // act - an id is reused by nothing here, but a note kept forever would still filter it
        parts.OrdersLoader.Emit([OpenOrder("1")]);

        // assert
        var snapshot = await ReadOrderAsync(orders, ct);
        snapshot.Items.Count.Is(1, "the note about an ended order outlived the snapshot that retired it");
    }

    /// <summary>
    /// The notes about ended orders are bounded when no snapshot arrives to retire them.
    /// </summary>
    /// <remarks>
    /// Notes are normally retired by the next snapshot, so a handful exist at a time. They are not
    /// retired at all while snapshots fail - a reload refused by a rate limit does that for minutes at
    /// a stretch, which was observed - and the stream goes on ending orders throughout. The cap is what
    /// keeps that from growing without end.
    ///
    /// Dropping the oldest costs nothing: a note guards against a snapshot already in flight, and the
    /// first snapshot to arrive after the loader recovers was sent later than every note held here, so
    /// it lists none of them.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task EndedOrderNotes_AreBoundedWhenNoSnapshotRetiresThem()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var orders = Channel.CreateUnbounded<ChangeEvent<OrderModel>>();
        using var subscription = connector.Orders.Subscribe(x => orders.Writer.TryWrite(x));
        connector.Sync();

        // act - one more order ends than the connector keeps notes for, and no snapshot arrives meanwhile
        const int cap = 1000;
        for (var i = 0; i <= cap; i++)
            parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("CANCELED", "0", (i + 1).ToString())));

        for (var i = 0; i <= cap; i++)
            (await ReadOrderAsync(orders, ct)).Type.Is(ChangeEventType.Delete);

        // assert - the newest note still holds, the oldest has been dropped to make room for it
        parts.OrdersLoader.Emit([OpenOrder("1"), OpenOrder((cap + 1).ToString())]);
        var snapshot = await ReadOrderAsync(orders, ct);
        snapshot.Items.Count.Is(1, "the notes about ended orders are not bounded, so they grow without end");
        snapshot.Items.Single().Id.Is("1", "the note dropped to stay within the bound was not the oldest");
    }

    /// <summary>
    /// Builds an open order as a snapshot would carry it.
    /// </summary>
    /// <param name="id">The exchange-assigned id.</param>
    /// <returns>The order.</returns>
    private static OrderModel OpenOrder(string id) =>
        new(
            id,
            "2f1d4e6a-8b3c-4d5e-9f01-23456789abcd",
            OrientationRange.Both,
            "BTCUSDT",
            OrderSide.Buy,
            OrderType.Limit,
            1m,
            100m,
            0m,
            false,
            1700000000000,
            OrderStatus.New,
            0m,
            0m,
            1700000000000
        );

    /// <summary>
    /// Builds a Binance conditional order update event.
    /// </summary>
    /// <param name="status">The status the event reports.</param>
    /// <returns>The event payload.</returns>
    private static string AlgoUpdate(string status) =>
        $$$"""
            {"e":"ALGO_UPDATE","T":1700000000000,"E":1700000000000,"o":{
            "caid":"5af8fa4d-1d36-4be4-b7cd-04fe48dcb0f5","aid":4000001912557457,"at":"CONDITIONAL",
            "o":"STOP","s":"BTCUSDT","S":"SELL","ps":"BOTH","f":"GTC","q":"1","X":"{{{status}}}","ai":"",
            "tp":"90","p":"89","V":"EXPIRE_MAKER","wt":"CONTRACT_PRICE","pm":"NONE","cp":false,
            "pP":false,"R":false,"tt":0,"gtd":0,"ia":false}}
            """;

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
    /// <param name="id">The exchange-assigned order id the event reports.</param>
    /// <returns>The event payload.</returns>
    private static string OrderUpdate(string status, string executedQty, string id = "1") =>
        $$$"""
            {"e":"ORDER_TRADE_UPDATE","E":1700000000000,"T":1700000000000,"o":{
            "s":"BTCUSDT","c":"2f1d4e6a-8b3c-4d5e-9f01-23456789abcd","S":"BUY","o":"LIMIT","f":"GTC",
            "q":"1","p":"100","ap":"0","sp":"0","x":"NEW","X":"{{{status}}}","i":{{{id}}},"l":"0","z":"{{{executedQty}}}",
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
