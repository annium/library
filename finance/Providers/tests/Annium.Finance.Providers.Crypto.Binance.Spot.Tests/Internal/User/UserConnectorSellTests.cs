using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.User;
using Xunit;
using static Annium.Finance.Providers.Abstractions.Domain.User.Requests.RequestBuilder;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User;

/// <summary>
/// Drives the sell side of the Binance spot user connector against the real account; in the <b>write</b>
/// block.
/// </summary>
/// <remarks>
/// <para>
/// A separate class because it needs a separate instrument. A resting sell sets aside the <b>base</b>
/// asset rather than the quote, so it can only be placed on a symbol whose base the account already
/// holds - and buying one to test selling it would mean a fill, which is the thing this whole block is
/// built to avoid.
/// </para>
/// <para>
/// So the symbol here is chosen for what the account holds, not for what the rest of the suite trades.
/// If this class ever fails at setup with the venue refusing for want of balance, that is the account
/// having changed rather than the connector having broken.
/// </para>
/// <para>
/// Worth having rather than assumed from the buy side: which asset an order locks is the one thing that
/// differs between the two, and a suite that only ever buys pins the quote half of a two-sided fact.
/// </para>
/// </remarks>
public class UserConnectorSellTests : SpotUserConnectorTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectorSellTests"/> class, targeting an
    /// instrument whose base asset the account holds.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public UserConnectorSellTests(ITestOutputHelper outputHelper)
        : base(Settings.User, "TRXUSDT", outputHelper) { }

    /// <summary>
    /// Registers the Binance spot provider, with the scheduled reload slow enough to fit the weight budget.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot(
            new ProviderConfiguration
            {
                ReloadContext = new CompositeLoaderConfig(200, 5, 1000, 5_000, 100),
                ReloadOrders = new CompositeLoaderConfig(200, 5, 1000, 15_000, 100),
                ReloadTrades = new CompositeLoaderConfig(200, 5, 1000, 15_000, 100),
            }
        );
    }

    /// <summary>
    /// A resting sell locks the base asset, not the quote, and releases it when cancelled.
    /// </summary>
    /// <remarks>
    /// The assertion is on the base asset deliberately. Asserted on the quote it would fail, and a test
    /// written to assert "some balance moved" would pass on either - which is how a suite ends up having
    /// checked one side twice.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.WriteTimeoutMs)]
    public async Task ARestingSell_LocksTheBaseAssetAndReleasesIt()
    {
        var ct = TestContext.Current.CancellationToken;
        var price = RestingSellPrice();
        var qty = RestingQty(price);
        var locked = LockedResource(OrderSide.Sell);

        // act
        var order = await InitValidOrder(
            InitLimitOrder(ClientOrderId(), Range(), Symbol, OrderSide.Sell, qty, price),
            OrderStatus.New,
            ct
        );

        // assert - the asset it would deliver is set aside while it rests
        await EnsureBalanceIsLocked(locked);

        // act
        await CancelValidOrder(order, ct);

        // assert - and given back when it stops resting
        await EnsureBalanceIsReleased(locked);
    }
}
