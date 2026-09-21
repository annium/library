using System.Threading.Tasks;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.User;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User;

/// <summary>
/// Runs <see cref="UserProviderTestBase"/>'s context/orders/trades checks against the real Binance Spot user
/// provider for BTCUSDT; in the <b>read</b> block, which it inherits from the base.
/// </summary>
/// <remarks>
/// Every case here was skipped with "Not implemented" until 2026-09-21, because the four read paths it
/// drives returned an empty success without issuing a request. They exist now and these run.
/// </remarks>
public class UserProviderTests : UserProviderTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserProviderTests"/> class, targeting BTCUSDT under the
    /// configured <see cref="Settings.User"/> account.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public UserProviderTests(ITestOutputHelper outputHelper)
        : base(Settings.User, "BTCUSDT", outputHelper) { }

    /// <summary>
    /// Registers the Binance Spot provider, with tight reload-loader intervals, so the user provider under
    /// test is resolved from its actual registration.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot(
            new ProviderConfiguration
            {
                ReloadContext = new CompositeLoaderConfig(200, 5, 1000, 1000, 100),
                ReloadOrders = new CompositeLoaderConfig(200, 5, 1000, 1000, 100),
                ReloadTrades = new CompositeLoaderConfig(200, 5, 1000, 1000, 100),
            }
        );
    }

    /// <summary>Reads the live account; in the <b>read</b> block.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public Task LoadContextAsync() => LoadContextBaseAsync(TestContext.Current.CancellationToken);

    /// <summary>Reads the live account; in the <b>read</b> block.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public Task LoadOpenOrdersAsync() => LoadOpenOrdersBaseAsync(TestContext.Current.CancellationToken);

    /// <summary>Reads the live account; in the <b>read</b> block.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public Task LoadLatestOrdersAsync() => LoadLatestOrdersBaseAsync(TestContext.Current.CancellationToken);

    /// <summary>Reads the live account; in the <b>read</b> block.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public Task LoadHistoryOrdersAsync() => LoadHistoryOrdersBaseAsync(TestContext.Current.CancellationToken);

    /// <summary>Reads the live account; in the <b>read</b> block.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public Task LoadLatestTradesAsync() => LoadLatestTradesBaseAsync(TestContext.Current.CancellationToken);

    /// <summary>Reads the live account; in the <b>read</b> block.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public Task LoadHistoryTradesAsync() => LoadHistoryTradesBaseAsync(TestContext.Current.CancellationToken);
}
