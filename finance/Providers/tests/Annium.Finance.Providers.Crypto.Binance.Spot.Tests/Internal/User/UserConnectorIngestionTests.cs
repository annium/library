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

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User;

/// <summary>
/// Pins what the Binance spot connector does with what arrives: an account event, a load from one of its
/// loaders, and the stream going up and down.
/// </summary>
/// <remarks>
/// There is no live trading block for this venue, so these branches are pinned here or nowhere. And even
/// with one, a live run cannot choose the message - a fill, a cancellation, an event arriving out of order
/// - and the handling of each is a separate branch.
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
    /// A balance event reloads the account context rather than being published as it stands.
    /// </summary>
    /// <remarks>
    /// The event carries only the balances that changed, where the connector publishes the account's assets
    /// as a full snapshot. Written straight through, it would replace the account with the fragment that
    /// happened to move - and an asset that did not change would vanish from the caller's view of it.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task ABalanceEvent_ReloadsTheContext()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var assets = Channel.CreateUnbounded<ChangeEvent<AssetModel>>();
        using var subscription = connector.Assets.Subscribe(x => assets.Writer.TryWrite(x));
        connector.Sync();

        // act
        parts.Stream.Push(Encoding.UTF8.GetBytes(BalanceUpdate));

        // assert
        await parts.ContextLoader.Requests.Reader.ReadAsync(ct);

        // and nothing was published from the event itself
        await Task.Delay(50, ct);
        assets.Reader.Count.Is(0);
    }

    /// <summary>
    /// A message that is neither of the two events the connector reads changes nothing.
    /// </summary>
    /// <remarks>
    /// Driven by a real event afterwards, so this asserts that the stream still works rather than that
    /// nothing observable happened quickly.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task AnUnknownEvent_IsIgnoredAndTheNextOneIsNot()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);

        // act
        parts.Stream.Push(Encoding.UTF8.GetBytes("""{"e":"somethingElse","E":1700000000000}"""));
        await Task.Delay(50, ct);
        var ignored = parts.ContextLoader.Requests.Reader.Count;

        parts.Stream.Push(Encoding.UTF8.GetBytes(BalanceUpdate));

        // assert
        ignored.Is(0);
        await parts.ContextLoader.Requests.Reader.ReadAsync(ct);
    }

    /// <summary>
    /// A fill asks for the symbol's trades; an order that has filled nothing does not.
    /// </summary>
    /// <remarks>
    /// The reload is what publishes a trade, even though this venue's order event carries the fill's price,
    /// quantity and commission. One source rather than two: published from both, every fill would reach a
    /// caller twice, and the deduplication that would then be needed is a worse thing to get wrong than a
    /// reload is to wait for.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task AFill_RequestsTheSymbolsTradesAndAnEmptyOrderDoesNot()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);

        // act
        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("NEW", "0")));
        await Task.Delay(50, ct);
        var afterNew = parts.TradesLoader.Requests.Reader.Count;

        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("FILLED", "1")));

        // assert
        afterNew.Is(0);
        (await parts.TradesLoader.Requests.Reader.ReadAsync(ct)).Is("BTCUSDT");
    }

    /// <summary>
    /// An order still open is published as a set; one that is over is published as a delete.
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
        var placed = await orders.Reader.ReadAsync(ct);
        placed.Type.Is(ChangeEventType.Set);
        placed.Item.Symbol.Is("BTCUSDT");
        placed.Item.Status.Is(OrderStatus.New);

        var gone = await orders.Reader.ReadAsync(ct);
        gone.Type.Is(ChangeEventType.Delete);
        gone.Item.Status.Is(OrderStatus.Canceled);
    }

    /// <summary>
    /// A snapshot older than a cancellation does not raise the order it still lists.
    /// </summary>
    /// <remarks>
    /// A snapshot describes the account as of the moment its request was sent, not the moment its answer
    /// arrived. Published as it arrives, a request sent just before an order ended raises an order the
    /// caller was already told was gone - and since the order is absent from every later snapshot and every
    /// later event, nothing ever corrects it.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task AStaleSnapshot_DoesNotRaiseAnOrderTheStreamAlreadyEnded()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var orders = Channel.CreateUnbounded<ChangeEvent<OrderModel>>();
        using var subscription = connector.Orders.Subscribe(x => orders.Writer.TryWrite(x));
        connector.Sync();

        // act
        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("CANCELED", "0")));
        var gone = await orders.Reader.ReadAsync(ct);
        gone.Type.Is(ChangeEventType.Delete);

        // a snapshot whose request left before the cancellation, arriving after it
        parts.OrdersLoader.Emit([Order("1", OrderStatus.New)]);

        // assert
        var snapshot = await orders.Reader.ReadAsync(ct);
        snapshot.Type.Is(ChangeEventType.Init);
        snapshot.Items.Count.Is(0, "a cancelled order was raised again by a stale snapshot");
    }

    /// <summary>
    /// An event reporting an order live, arriving after the event that ended it, is ignored.
    /// </summary>
    /// <remarks>
    /// The venue's events are not ordered against each other, so a placement can land after the
    /// cancellation of the same order. Over is final here and no id is reused, so the report that came
    /// first in time stands, whichever arrived last.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task ALateLiveEvent_DoesNotRaiseAnOrderAlreadyEnded()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var orders = Channel.CreateUnbounded<ChangeEvent<OrderModel>>();
        using var subscription = connector.Orders.Subscribe(x => orders.Writer.TryWrite(x));
        connector.Sync();

        // act
        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("CANCELED", "0")));
        (await orders.Reader.ReadAsync(ct)).Type.Is(ChangeEventType.Delete);

        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("NEW", "0")));
        await Task.Delay(50, ct);
        var afterLateEvent = orders.Reader.Count;

        // a different order still gets through, so this is not a connector that stopped publishing
        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("NEW", "0", "2")));

        // assert
        afterLateEvent.Is(0, "a cancelled order was raised again by a late event");
        (await orders.Reader.ReadAsync(ct)).Type.Is(ChangeEventType.Set);
    }

    /// <summary>
    /// A note about an ended order is dropped by the first snapshot that agrees it is gone.
    /// </summary>
    /// <remarks>
    /// Exact rather than approximate because loads are serialised: the next request is sent only after the
    /// previous answer has been delivered, so a note has to survive exactly one in-flight snapshot.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task ANote_IsForgottenOnceTheVenueAgrees()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var orders = Channel.CreateUnbounded<ChangeEvent<OrderModel>>();
        using var subscription = connector.Orders.Subscribe(x => orders.Writer.TryWrite(x));
        connector.Sync();

        parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("CANCELED", "0")));
        (await orders.Reader.ReadAsync(ct)).Type.Is(ChangeEventType.Delete);

        // a snapshot that no longer lists it retires the note
        parts.OrdersLoader.Emit([]);
        (await orders.Reader.ReadAsync(ct)).Items.Count.Is(0);

        // act - an id may be listed again only because the note is gone, not because it was reused
        parts.OrdersLoader.Emit([Order("1", OrderStatus.New)]);

        // assert
        var snapshot = await orders.Reader.ReadAsync(ct);
        snapshot.Items.Count.Is(1, "the note outlived the snapshot that retired it");
    }

    /// <summary>
    /// The notes are capped, so a stream that keeps going while reloads fail does not grow without bound.
    /// </summary>
    /// <remarks>
    /// Not hypothetical: a reload refused by a rate limit stops snapshots arriving for minutes while the
    /// stream carries on. A note that old protects nothing anyway - it guards against a snapshot already in
    /// flight, and the one that arrives when the loader recovers is newer than every note held.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task TheNotes_AreBoundedWhenNoSnapshotRetiresThem()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var orders = Channel.CreateUnbounded<ChangeEvent<OrderModel>>();
        using var subscription = connector.Orders.Subscribe(x => orders.Writer.TryWrite(x));
        connector.Sync();

        // act - more ended orders than the cap, with no snapshot in between to retire any of them
        const int cap = 1000;
        for (var i = 1; i <= cap + 1; i++)
            parts.Stream.Push(Encoding.UTF8.GetBytes(OrderUpdate("CANCELED", "0", i.ToString())));

        for (var i = 0; i < cap + 1; i++)
            (await orders.Reader.ReadAsync(ct)).Type.Is(ChangeEventType.Delete);

        // three ids, one from each end of the window and one from inside it: the oldest, whose note the
        // cap has dropped; the newest, still held; and one just inside the cap, which is what tells a cap
        // of this size from a cap of one - both drop the oldest, only this size still holds the middle
        parts.OrdersLoader.Emit([
            Order("1", OrderStatus.New),
            Order("2", OrderStatus.New),
            Order((cap + 1).ToString(), OrderStatus.New),
        ]);

        // assert
        var snapshot = await orders.Reader.ReadAsync(ct);
        snapshot.Items.Count.Is(1, "the notes were capped at the wrong size");

        // the oldest is the one the cap gave up, which is the price of bounding it and is stated here
        // rather than discovered: a note only ever guards against a snapshot already in flight
        snapshot.Items.Single().Id.Is("1");
    }

    /// <summary>
    /// The stream going up starts the loaders, and going down stops them.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task TheStreamLifecycle_StartsAndStopsTheLoaders()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);

        // act
        parts.Stream.Connect();
        await Task.Delay(50, ct);
        var startedContext = parts.ContextLoader.Starts;
        var startedOrders = parts.OrdersLoader.Starts;

        parts.Stream.Disconnect();
        await Task.Delay(50, ct);

        // assert
        startedContext.Is(1);
        startedOrders.Is(1);
        parts.ContextLoader.Stops.Is(1);
        parts.OrdersLoader.Stops.Is(1);
    }

    /// <summary>
    /// The account context publishes an empty position set, because a spot account cannot hold one.
    /// </summary>
    /// <remarks>
    /// Published rather than skipped: a consumer reading both collections would otherwise wait forever on
    /// one that is never written. The emptiness is the venue's answer, not a gap in ours.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = Timeout)]
    public async Task TheContext_PublishesAnEmptyPositionSet()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = RunServer();
        await using var connector = CreateConnector(server, out var parts);
        var positions = Channel.CreateUnbounded<ChangeEvent<PositionModel>>();
        var assets = Channel.CreateUnbounded<ChangeEvent<AssetModel>>();
        using var positionSubscription = connector.Positions.Subscribe(x => positions.Writer.TryWrite(x));
        using var assetSubscription = connector.Assets.Subscribe(x => assets.Writer.TryWrite(x));
        connector.Sync();

        // act
        parts.ContextLoader.Emit(new UserContext([new AssetModel("USDT", 100m, 100m)], Array.Empty<PositionModel>()));

        // assert
        (await assets.Reader.ReadAsync(ct)).Items.Count.Is(1);

        var published = await positions.Reader.ReadAsync(ct);
        published.Type.Is(ChangeEventType.Init);
        published.Items.Count.Is(0);
    }

    /// <summary>
    /// A recorded <c>outboundAccountPosition</c> event.
    /// </summary>
    private const string BalanceUpdate = """
        {"e":"outboundAccountPosition","E":1700000000000,"u":1700000000000,
        "B":[{"a":"USDT","f":"100.00","l":"0.00"}]}
        """;

    /// <summary>
    /// Builds an <c>executionReport</c> event.
    /// </summary>
    /// <param name="status">The order status the event reports.</param>
    /// <param name="executedQty">The cumulative filled quantity the event reports.</param>
    /// <param name="id">The exchange-assigned order id the event reports.</param>
    /// <returns>The event payload.</returns>
    private static string OrderUpdate(string status, string executedQty, string id = "1") =>
        $$$"""
            {"e":"executionReport","E":1700000000000,"s":"BTCUSDT","c":"order-{{{id}}}","S":"BUY",
            "o":"LIMIT","f":"GTC","q":"1.00","p":"100.00","P":"0.00","F":"0.00","g":-1,"C":"",
            "x":"NEW","X":"{{{status}}}","r":"NONE","i":{{{id}}},"l":"0.00","z":"{{{executedQty}}}",
            "L":"0.00","n":"0.00","N":"USDT","T":1700000000000,"t":-1,"I":1,"w":true,"m":false,
            "M":false,"O":1700000000000,"Z":"0.00","Y":"0.00","Q":"0.00","W":1700000000000,"V":"NONE"}
            """;

    /// <summary>
    /// An order as a snapshot would carry it.
    /// </summary>
    /// <param name="id">The order's exchange-assigned id.</param>
    /// <param name="status">The order's status.</param>
    /// <returns>The order.</returns>
    private static OrderModel Order(string id, OrderStatus status) =>
        new(
            id,
            $"order-{id}",
            OrientationRange.Both,
            "BTCUSDT",
            OrderSide.Buy,
            OrderType.Limit,
            1m,
            100m,
            0m,
            false,
            1_700_000_000_000,
            status,
            0m,
            0m,
            1_700_000_000_000
        );

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
