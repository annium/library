using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Data.Tables;
using Annium.Finance.Providers.Abstractions.Connectors.Market;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Abstractions.Domain.User.Requests;
using Annium.Finance.Providers.Tests.Lib.Shared.Operations;
using Annium.Finance.Providers.Tests.Lib.User.Operations;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Tests.Lib.User;

/// <summary>
/// Drives a user connector against a real <b>cash</b> account - one that holds balances and no positions -
/// so the order lifecycle can be exercised on a venue where there is nothing to flatten.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is not <see cref="UserConnectorTestBase"/>.</b> That fixture is built around positions
/// existing: it waits for a position snapshot before it will start, checks the account's position mode, and
/// closes what it finds. A cash account reports no positions ever, so the wait never ends and the mode
/// check throws. Reaching for it here would not have produced a wrong test, it would have produced one that
/// hangs until its deadline.
/// </para>
/// <para>
/// <b>Cleanup cancels orders and nothing else - it must never sell.</b> On a margined account, flattening a
/// position is restorative: it returns the account to where it started. On a cash account the balances
/// <em>are</em> the holdings, so the same instinct - "leave the account flat" - liquidates whatever the
/// account owns. There is no state here that cleanup is entitled to undo except the orders this fixture
/// itself placed.
/// </para>
/// <para>
/// <b>Every order placed here rests far from the market and cannot fill.</b> A limit buy priced well below
/// the book exercises placement, the stream reporting it, the balance locking behind it, replacement and
/// cancellation - and buys nothing, sells nothing, and pays no fee, because an order that never fills has
/// no fee to pay. What it does not exercise is a fill, and that is stated rather than worked around: on a
/// cash account a fill means actually buying, which changes holdings and costs money, and it is a decision
/// rather than a detail.
/// </para>
/// </remarks>
[Trait(TestBlock.Name, TestBlock.Write)]
public abstract class SpotUserConnectorTestBase : ProvidersTestBase, IAsyncLifetime
{
    /// <summary>Gets the instrument under test, as the venue describes it.</summary>
    protected InstrumentModel Instrument { get; private set; } = null!;

    /// <summary>Gets the symbol under test.</summary>
    protected string Symbol { get; }

    /// <summary>Gets the last ticker seen for <see cref="Symbol"/>, which prices every order placed here.</summary>
    protected InstrumentTicker Ticker { get; private set; }

    /// <summary>The connector under test.</summary>
    private IUserConnector Connector { get; set; } = null!;

    /// <summary>The credentials the connector authenticates with.</summary>
    private readonly UserSettings _settings;

    /// <summary>Every asset balance reported, snapshots and updates alike.</summary>
    private readonly ConcurrentQueue<AssetModel> _assets = new();

    /// <summary>Every order reported, snapshots and updates alike.</summary>
    private readonly ConcurrentQueue<OrderModel> _orders = new();

    /// <summary>Everything reported on the connector's error channel.</summary>
    private readonly ConcurrentQueue<ConnectorError> _errors = new();

    /// <summary>The balance as of the last <see cref="Snapshot"/>, which the locked/released checks compare against.</summary>
    private AssetModel _balance = null!;

    /// <summary>Everything this fixture has to tear down.</summary>
    private AsyncDisposableBox _disposable = null!;

    /// <summary>Whether the connector was built, and therefore whether there is anything to clean up with.</summary>
    private bool _connectorBuilt;

    /// <summary>Completes when the orders loader has delivered its first snapshot.</summary>
    /// <remarks>
    /// Needed because an empty snapshot and a snapshot that has not arrived look identical from the queue:
    /// neither enqueues anything. The difference decides whether there is anything to cancel, and asking
    /// the venue to cancel nothing is an error on this venue rather than a no-op.
    /// </remarks>
    private readonly TaskCompletionSource _ordersSnapshot = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Initializes a new instance of the <see cref="SpotUserConnectorTestBase"/> class.
    /// </summary>
    /// <param name="settings">The account credentials the connector authenticates with.</param>
    /// <param name="symbol">The symbol to trade.</param>
    /// <param name="output">The xUnit output helper to route trace logging to.</param>
    protected SpotUserConnectorTestBase(UserSettings settings, string symbol, ITestOutputHelper output)
        : base(output)
    {
        _settings = settings;
        Symbol = symbol;
    }

    /// <summary>
    /// Builds the market and user connectors, waits for both, cancels anything already open on the symbol,
    /// and waits for the account's balances.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async ValueTask InitializeAsync()
    {
        // the base builds the provider; everything below resolves from it - the logger included - so it
        // has to run first
        await base.InitializeAsync();

        _disposable = Disposable.AsyncBox(Logger);

        this.Trace("start");

        this.Trace("get market connector");
        var marketFactory = Get<IMarketConnectorFactory>();
        var marketSettings = Get<IMapper>().Map<MarketSettings>(_settings);
        var market = marketFactory.Create(marketSettings);
        _disposable += market;

        this.Trace("await until market connector is ready");
        await market.WhenConnectedAsync(TestContext.Current.CancellationToken);

        this.Trace<string>("find instrument {symbol}", Symbol);
        Instrument = market.Instruments.Single(x => x.Symbol == Symbol);

        // the ticker is what prices every order this fixture places, so it is a precondition rather than
        // a convenience: without it there is no way to say "far below the market" at all
        this.Trace<string>("subscribe and wait for ticker for {symbol}", Symbol);
        market.SubscribeTickers([Symbol]);
        Ticker = await market.Tickers.FirstAsync(x => x.Symbol == Symbol).ToTask(TestContext.Current.CancellationToken);

        this.Trace("get user connector for {settings}", _settings);
        var userFactory = Get<IUserConnectorFactory>();
        _disposable += Connector = userFactory.Create(_settings);

        // from here on the account is reachable, so teardown must attempt its cleanup even if what follows
        // throws
        _connectorBuilt = true;

        this.Trace("subscribe to connector data");
        _disposable += Connector.Assets.Subscribe(x => Collect(_assets, x));
        _disposable += Connector.Orders.Subscribe(x =>
        {
            Collect(_orders, x);
            if (x.Type is ChangeEventType.Init)
                _ordersSnapshot.TrySetResult();
        });

        this.Trace("await until user connector is ready");
        await Connector.WhenConnectedAsync(TestContext.Current.CancellationToken);

        // counted from here, not from before the wait: getting connected is allowed to take more than one
        // attempt, and each failed handshake raises an error the socket then recovers from by retrying
        this.Trace("subscribe to connector errors");
        Connector.OnError += _errors.Enqueue;

        this.Trace("cancel open orders");
        await CancelOpenOrders(TestContext.Current.CancellationToken);

        this.Trace("await for balances");
        await AwaitForInitialBalances(TestContext.Current.CancellationToken);

        EnsureNoErrors();

        this.Trace("done");
    }

    /// <summary>
    /// Cancels every open order on <see cref="Symbol"/> and disposes the connector. <b>Sells nothing.</b>
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async ValueTask DisposeAsync()
    {
        this.Trace("start");

        // cleanup and disposal are separate obligations and neither may cancel the other: cleanup starts by
        // talking to the exchange, and a throw there must not leave the container unreleased - while a
        // failure to leave the account as it was found is the thing most worth reporting
        Exception? cleanupError = null;
        Exception? disposeError = null;

        try
        {
            await CleanUpAccountAsync();
        }
        catch (Exception e)
        {
            cleanupError = e;
        }

        try
        {
            this.Trace("dispose disposables");
            if (_disposable is not null)
                await _disposable.DisposeAsync();

            await base.DisposeAsync();
        }
        catch (Exception e)
        {
            disposeError = e;
        }

        if (cleanupError is not null && disposeError is not null)
            throw new AggregateException(
                "account cleanup and disposal both failed; orders may still be open",
                cleanupError,
                disposeError
            );

        if (cleanupError is not null)
            ExceptionDispatchInfo.Capture(cleanupError).Throw();

        if (disposeError is not null)
            ExceptionDispatchInfo.Capture(disposeError).Throw();

        EnsureNoErrors();

        this.Trace("done");
    }

    /// <summary>
    /// Leaves the account as this fixture found it: every open order on <see cref="Symbol"/> cancelled.
    /// </summary>
    /// <remarks>
    /// That is the whole of it, deliberately. Nothing else here belongs to this fixture: the balances were
    /// the account's before it ran and are the account's after, and no amount of tidying is entitled to
    /// spend them.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CleanUpAccountAsync()
    {
        // nothing to clean with, and nothing placed either: the connector is what talks to the account
        if (!_connectorBuilt)
        {
            this.Trace("skip account cleanup, the connector was never built");

            return;
        }

        this.Trace("cancel open orders");
        await CancelOpenOrders(TestContext.Current.CancellationToken);

        EnsureNoErrors();
    }

    /// <summary>
    /// Asserts that the connector has not raised any errors so far.
    /// </summary>
    protected void EnsureNoErrors()
    {
        // named, because "expected to be empty, but has 4 items" sends the reader to the log to find out
        // what the four were - and the log of a live run is tens of thousands of lines
        _errors.IsEmpty($"connector reported: {string.Join("; ", _errors.Select(x => x.Message))}");
    }

    /// <summary>
    /// Records the current balance, so the locked/released checks have something to compare against.
    /// </summary>
    protected void Snapshot()
    {
        this.Trace("start");

        _balance = GetBalance(Instrument.Quote.Code);

        this.Trace("done");
    }

    /// <summary>
    /// Places a real order and asserts it comes back matching the request and reported with the expected status.
    /// </summary>
    /// <param name="request">The order-init request to place.</param>
    /// <param name="status">The status the order is expected to be reported with.</param>
    /// <param name="ct">The test's cancellation token, which its deadline signals.</param>
    /// <returns>The placed order as reported by the connector.</returns>
    protected async Task<OrderModel> InitValidOrder(IInitOrderRequest request, OrderStatus status, CancellationToken ct)
    {
        this.Trace("start");

        Snapshot();

        this.Trace("execute start");
        var order = await Connector.InitOrderAsync(request).UnwrapAsync().WaitAsync(ct);
        this.Trace("execute done");

        EnsureNoErrors();

        order.ShouldMatch(request);
        await EnsureOrderReported(order, status);

        EnsureNoErrors();

        this.Trace("done");

        return order;
    }

    /// <summary>
    /// Sends an order the exchange is expected to refuse, and asserts that it fails.
    /// </summary>
    /// <param name="request">The order-init request expected to be refused.</param>
    /// <param name="ct">The test's cancellation token, which its deadline signals.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task InitInvalidOrder(IInitOrderRequest request, CancellationToken ct)
    {
        this.Trace("start");

        await Connector.InitOrderAsync(request).EnsureFailedAsync().WaitAsync(ct);

        EnsureNoErrors();

        this.Trace("done");
    }

    /// <summary>
    /// Replaces a real order and asserts the replacement comes back matching the request.
    /// </summary>
    /// <param name="request">The modify request to send.</param>
    /// <param name="status">The status the resulting order is expected to be reported with.</param>
    /// <param name="ct">The test's cancellation token, which its deadline signals.</param>
    /// <returns>The resulting order as reported by the connector.</returns>
    protected async Task<OrderModel> ModifyValidOrder(
        IModifyOrderRequest request,
        OrderStatus status,
        CancellationToken ct
    )
    {
        this.Trace("start");

        Snapshot();

        this.Trace("execute start");
        var order = await Connector.ModifyOrderAsync(request).UnwrapAsync().WaitAsync(ct);
        this.Trace("execute done");

        EnsureNoErrors();

        order.ShouldMatch(request);
        await EnsureOrderReported(order, status);

        EnsureNoErrors();

        this.Trace("done");

        return order;
    }

    /// <summary>
    /// Cancels a real order and asserts it comes back reported as cancelled.
    /// </summary>
    /// <param name="order">The order to cancel.</param>
    /// <param name="ct">The test's cancellation token, which its deadline signals.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task CancelValidOrder(OrderModel order, CancellationToken ct)
    {
        this.Trace("start");

        Snapshot();

        this.Trace("execute start");
        await Connector.CancelOrderAsync(RequestBuilder.CancelOrder(order)).UnwrapAsync().WaitAsync(ct);
        this.Trace("execute done");

        EnsureNoErrors();

        await EnsureOrderReported(order, OrderStatus.Canceled);

        EnsureNoErrors();

        this.Trace("done");
    }

    /// <summary>
    /// Cancels every open order on <see cref="Symbol"/> on the real account.
    /// </summary>
    /// <param name="ct">The test's cancellation token, which its deadline signals.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task CancelOpenOrders(CancellationToken ct)
    {
        this.Trace("cancel all orders - start");

        // the venue refuses a cancel-all on a symbol with nothing open - measured 2026-09-21, HTTP 400 with
        // the code its reference calls CANCEL_REJECTED and a message saying the order could not be found.
        // That code is a family rather than a reason: the same one carries "market is closed" and "this
        // account may not place or cancel orders", so it cannot be folded into success without swallowing
        // the two failures a cleanup path least wants swallowed. Asking only when there is something to
        // cancel is the honest way round it
        await _ordersSnapshot.Task.WaitAsync(ct);

        var open = CurrentlyOpenOrders();
        if (open.Length == 0)
        {
            this.Trace<string>("skip cancel all orders - nothing open on {symbol}", Symbol);

            return;
        }

        this.Trace<string, string>("cancel {count} open order(s) on {symbol}", open.Length.ToString(), Symbol);

        await Connector.CancelAllOrdersAsync(Symbol).UnwrapAsync().WaitAsync(ct);

        EnsureNoErrors();

        this.Trace("cancel all orders - done");
    }

    /// <summary>
    /// The orders on <see cref="Symbol"/> that the connector last reported as open.
    /// </summary>
    /// <remarks>
    /// Taken as the last state reported per id rather than as everything reported, because the queue keeps
    /// the whole history: an order placed and then cancelled appears twice, and only the second says where
    /// it ended up.
    /// </remarks>
    /// <returns>The orders currently open.</returns>
    private OrderModel[] CurrentlyOpenOrders() =>
        [
            .. _orders
                .Where(x => x.Symbol == Symbol)
                .GroupBy(x => x.Id)
                .Select(g => g.Last())
                .Where(x => x.Status is OrderStatus.New or OrderStatus.PartiallyFilled),
        ];

    /// <summary>
    /// Waits until the connector has reported at least one asset balance, plus a grace period for further
    /// messages to settle.
    /// </summary>
    /// <param name="ct">The test's cancellation token, which its deadline signals.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task AwaitForInitialBalances(CancellationToken ct)
    {
        this.Trace("await for balances");
        await Expect.ToAsync(() => _assets.IsNotEmpty(), ct);

        // a grace period for messages already in flight to settle, so the first snapshot a test reads is
        // the account as it now stands rather than as it stood mid-cancellation
        this.Trace("await for messages");
        await Task.Delay(1000, ct);
    }

    /// <summary>
    /// A price far enough below the market that an order at it rests rather than fills, and near enough
    /// that the venue accepts it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The distance is bounded from <b>both</b> sides, which is the part that is easy to miss: a venue
    /// limits how far from the market a limit order may be priced, and an order too far away is refused by
    /// a filter rather than left to rest. Measured on the first live run of this block - half the market
    /// was refused outright.
    /// </para>
    /// <para>
    /// Three things made that failure exact rather than marginal, and all three are worth stating because
    /// only the first is obvious. The bound was <em>exactly</em> half. It is applied against an average of
    /// recent trade prices rather than against the bid this is computed from, so dividing the bid by the
    /// bound's own factor lands on the wrong side of it whenever the average is above the bid. And rounding
    /// down to the tick, which is the safe direction for not filling, is the unsafe direction for this.
    /// </para>
    /// <para>
    /// So this leaves room rather than aiming at the bound: well inside whatever the venue allows, and
    /// still far enough below the market that a test measured in minutes will not see it filled. The bound
    /// itself is not in <see cref="InstrumentModel"/> - the provider does not read that filter - so it
    /// cannot be computed here, and that gap is recorded in the provider manifest rather than guessed at.
    /// </para>
    /// </remarks>
    /// <returns>The price to rest at.</returns>
    protected decimal RestingPrice()
    {
        var price = Instrument.ToTickSizeDown(Ticker.BidPrice * 0.6m);

        return Math.Max(price, Instrument.MinPrice);
    }

    /// <summary>
    /// A second resting price, distinct from <see cref="RestingPrice"/>, for a replacement to move to.
    /// </summary>
    /// <remarks>
    /// Above the first rather than below it: the bound this has to stay inside is a floor, so moving up
    /// walks away from it. The order is still far enough below the market not to fill.
    /// </remarks>
    /// <returns>The price to move a replacement to.</returns>
    protected decimal ReplacementPrice()
    {
        var price = Instrument.ToTickSizeDown(Ticker.BidPrice * 0.7m);

        return Math.Max(price, Instrument.MinPrice);
    }

    /// <summary>
    /// The smallest quantity the venue accepts at the given price.
    /// </summary>
    /// <remarks>
    /// Sized from the instrument's own minimum notional rather than from a number chosen here, with a
    /// margin over it: a notional computed to land exactly on the minimum is refused by any rounding that
    /// goes the wrong way, and the refusal names a filter rather than the arithmetic that produced it.
    /// </remarks>
    /// <param name="price">The price the order will rest at.</param>
    /// <returns>The quantity to place.</returns>
    protected decimal RestingQty(decimal price)
    {
        var byNotional = Instrument.MinSum * 1.1m / price;

        return Instrument.ToValidQty(Math.Max(byNotional, Instrument.MinQty));
    }

    /// <summary>Generates a fresh, random client order id for a new request.</summary>
    /// <returns>A new random client order id.</returns>
    protected string ClientOrderId() => Guid.NewGuid().ToString();

    /// <summary>Gets the orientation range used for requests in this scenario.</summary>
    /// <returns>Always <see cref="OrientationRange.Both"/>; a cash account has no sides to hold.</returns>
    protected OrientationRange Range() => OrientationRange.Both;

    /// <summary>
    /// Gets the most recently reported balance for the given asset.
    /// </summary>
    /// <param name="resource">The asset code to get the balance of.</param>
    /// <returns>The most recently reported balance for the asset.</returns>
    protected AssetModel GetBalance(string resource)
    {
        this.Trace<string>("get {resource} last balance", resource);

        return _assets.Last(x => x.Resource == resource);
    }

    /// <summary>
    /// Asserts that the quote balance has more locked and less free than at the last <see cref="Snapshot"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected ValueTask EnsureBalanceIsLocked()
    {
        var originalBalance = _balance;

        this.Trace<string>(
            "ensure current balance is locked compared to original {balance}",
            JsonSerializer.Serialize(originalBalance)
        );

        return Expect.ToAsync(() =>
        {
            var currentBalance = GetBalance(Instrument.Quote.Code);
            currentBalance.Free.IsLess(originalBalance.Free);
            currentBalance.Locked.IsGreater(originalBalance.Locked);
        });
    }

    /// <summary>
    /// Asserts that the quote balance has more free and less locked than at the last <see cref="Snapshot"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected ValueTask EnsureBalanceIsReleased()
    {
        var originalBalance = _balance;

        this.Trace<string>(
            "ensure current balance is released compared to original {balance}",
            JsonSerializer.Serialize(originalBalance)
        );

        return Expect.ToAsync(() =>
        {
            var currentBalance = GetBalance(Instrument.Quote.Code);
            currentBalance.Free.IsGreater(originalBalance.Free);
            currentBalance.Locked.IsLess(originalBalance.Locked);
        });
    }

    /// <summary>
    /// Waits until the connector has reported the given order with the given status.
    /// </summary>
    /// <param name="order">The order to look for.</param>
    /// <param name="status">The status it is expected to be reported with.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private ValueTask EnsureOrderReported(OrderModel order, OrderStatus status)
    {
        this.Trace("ensure order {order} is reported and has status {status}", order.Id, status);

        return Expect.ToAsync(() =>
        {
            var orderMessage = _orders.Last(x => x.Id == order.Id);
            orderMessage.ShouldMatch(order);
            orderMessage.Status.Is(status);
        });
    }

    /// <summary>
    /// Adds a change event's payload to a queue, whether it arrived as a snapshot or as a single change.
    /// </summary>
    /// <typeparam name="T">The type of the item the change carries.</typeparam>
    /// <param name="queue">The queue to collect into.</param>
    /// <param name="change">The change event received.</param>
    private static void Collect<T>(ConcurrentQueue<T> queue, ChangeEvent<T> change)
        where T : notnull
    {
        if (change.Type is ChangeEventType.Init)
            foreach (var item in change.Items)
                queue.Enqueue(item);
        else
            queue.Enqueue(change.Item);
    }
}
