using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.User;
using Annium.Testing;
using Xunit;
using static Annium.Finance.Providers.Abstractions.Domain.User.Requests.RequestBuilder;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User;

/// <summary>
/// Drives the Binance spot user connector's order lifecycle against the real account; in the <b>write</b>
/// block.
/// </summary>
/// <remarks>
/// <para>
/// <b>Nothing here fills.</b> Every order is a limit buy priced at half the market, which rests until it is
/// cancelled. That exercises placement, the account stream reporting it, the balance locking behind it,
/// replacement and cancellation — and buys nothing, sells nothing and pays no fee. A cash account's
/// balances are its holdings, so a block that filled would be spending the account's money to assert
/// something the resting path already asserts.
/// </para>
/// <para>
/// <b>What that leaves unasserted, stated rather than hidden:</b> a fill, and everything downstream of one
/// — the trade the reload publishes, the commission on it, and a partially filled order's arithmetic. Those
/// are pinned offline against recorded payloads and are not pinned live here.
/// </para>
/// </remarks>
public class UserConnectorTests : SpotUserConnectorTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectorTests"/> class, targeting DOTUSDT under the
    /// configured <see cref="Settings.User"/> account.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public UserConnectorTests(ITestOutputHelper outputHelper)
        : base(Settings.User, "DOTUSDT", outputHelper) { }

    /// <summary>
    /// Registers the Binance spot provider. Account changes are observed through the debounce rather than
    /// through the scheduled reload, so the schedule can be slow enough to fit the weight budget.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot(
            new ProviderConfiguration
            {
                // the debounce is what these tests actually run on: a command and a stream event each ask
                // for a reload, and it arrives within it. The scheduled interval is the opposite concern -
                // it is what runs when nothing is happening, and on this venue the open-order list costs 80
                // because the connector asks for every symbol. At one second that is 4800 a minute against
                // a ceiling of 6000, which the contract states outright and a config copied from the other
                // venue walks straight into: the first run of this block spent the budget and the next
                // request was refused by the local limiter before it was sent
                ReloadContext = new CompositeLoaderConfig(200, 5, 1000, 5_000, 100),
                ReloadOrders = new CompositeLoaderConfig(200, 5, 1000, 15_000, 100),
                ReloadTrades = new CompositeLoaderConfig(200, 5, 1000, 15_000, 100),
            }
        );
    }

    /// <summary>
    /// A resting limit buy is placed, reported by the account stream, locks quote balance, and releases it
    /// when cancelled.
    /// </summary>
    /// <remarks>
    /// The balance assertions are what make this more than "the exchange accepted a request": they are
    /// observed through the account stream, so they fail if the stream is connected and silent — the one
    /// failure a connection-based transport cannot report about itself.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.WriteTimeoutMs)]
    public async Task ARestingOrder_IsPlacedReportedAndCancelled()
    {
        var ct = TestContext.Current.CancellationToken;
        var price = RestingPrice();
        var qty = RestingQty(price);

        // act
        var order = await InitValidOrder(
            InitLimitOrder(ClientOrderId(), Range(), Symbol, OrderSide.Buy, qty, price),
            OrderStatus.New,
            ct
        );

        // assert - the quote currency it would spend is set aside while it rests
        await EnsureBalanceIsLocked();

        // act
        await CancelValidOrder(order, ct);

        // assert - and given back when it stops resting
        await EnsureBalanceIsReleased();
    }

    /// <summary>
    /// A resting order is replaced, and the replacement is a different order at the new terms.
    /// </summary>
    /// <remarks>
    /// This venue has no amendment that can change a price: the connector sends one cancel-and-replace
    /// request, which is a new order at the back of the queue. The assertion that the id changed is what
    /// says so — an implementation that quietly amended would keep it.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.WriteTimeoutMs)]
    public async Task ARestingOrder_IsReplacedByANewOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var price = RestingPrice();
        var qty = RestingQty(price);

        var order = await InitValidOrder(
            InitLimitOrder(ClientOrderId(), Range(), Symbol, OrderSide.Buy, qty, price),
            OrderStatus.New,
            ct
        );

        // act - a different resting price, still far below the market and still inside the venue's own
        // limit on how far a limit order may be priced from it
        var newPrice = ReplacementPrice();
        var replaced = await ModifyValidOrder(
            ModifyToLimitOrder(order, OrderSide.Buy, RestingQty(newPrice), newPrice),
            OrderStatus.New,
            ct
        );

        // assert - a replacement, not an amendment: this venue cannot amend a price and does not pretend to
        replaced.Id.IsNot(order.Id, "the replacement carried the replaced order's id, so nothing was replaced");
        replaced.Price.Is(newPrice);

        // cleanup
        await CancelValidOrder(replaced, ct);
    }

    /// <summary>
    /// An order below the instrument's minimum notional is refused by the exchange and the refusal reaches
    /// the caller.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The cheapest live refusal there is, and the one worth having: the connector's success type for a
    /// placement is an order, and the defect this module has already met once is an exchange error being
    /// discarded whenever the caller's type can represent "nothing".
    /// </para>
    /// <para>
    /// The price is the same resting price the other cases use, which matters: on the first live run this
    /// case passed while the venue was refusing every order in the block for an unrelated filter. A test
    /// that asserts only "it was refused" passes on any refusal, so what it is priced at is the difference
    /// between asserting the notional and asserting nothing in particular.
    /// </para>
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.WriteTimeoutMs)]
    public async Task AnOrderBelowTheMinimumNotional_IsRefused()
    {
        var ct = TestContext.Current.CancellationToken;
        var price = RestingPrice();

        // a single lot, which against any meaningful minimum notional is below it. Sized from the
        // instrument rather than from a number written here
        await InitInvalidOrder(
            InitLimitOrder(ClientOrderId(), Range(), Symbol, OrderSide.Buy, Instrument.MinQty, price),
            ct
        );
    }

    /// <summary>
    /// Cancelling all orders on the symbol removes a resting order and releases what it locked.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.WriteTimeoutMs)]
    public async Task CancelAll_RemovesARestingOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var price = RestingPrice();
        var qty = RestingQty(price);

        await InitValidOrder(
            InitLimitOrder(ClientOrderId(), Range(), Symbol, OrderSide.Buy, qty, price),
            OrderStatus.New,
            ct
        );
        await EnsureBalanceIsLocked();

        // act
        Snapshot();
        await CancelOpenOrders(ct);

        // assert
        await EnsureBalanceIsReleased();
    }
}
