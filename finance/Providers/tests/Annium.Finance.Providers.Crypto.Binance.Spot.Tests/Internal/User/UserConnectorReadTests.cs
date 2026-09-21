using System.Threading.Tasks;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.User;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User;

/// <summary>
/// Connects a live Binance spot user connector to the real account and asserts it reaches connected and
/// delivers a snapshot. Places nothing; in the <b>read</b> block.
/// </summary>
/// <remarks>
/// The first stage of live validation for this connector and the only one that cannot cost anything. For a
/// user connector, reaching connected means more than an HTTP call succeeding: on this venue it means the
/// socket opened against the WebSocket API, a signed subscription was accepted, and every loader behind it
/// reported in. It is therefore the cheapest test that can tell the new transport works at all.
/// </remarks>
public class UserConnectorReadTests : UserConnectorReadTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectorReadTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public UserConnectorReadTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the Binance spot provider, with the scheduled reload slow enough to fit the weight budget
    /// and the debounce short enough that a snapshot still arrives promptly.
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

    /// <summary>Connects to the live account and reads it; in the <b>read</b> block.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public Task UserConnectorReadAsync() =>
        UserConnectorReadBaseAsync(Settings.User, TestContext.Current.CancellationToken);
}
