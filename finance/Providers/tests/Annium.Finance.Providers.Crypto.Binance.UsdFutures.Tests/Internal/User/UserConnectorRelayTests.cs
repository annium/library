using System;
using System.Threading.Tasks;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.Shared;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Finance.Providers.Tests.Lib.User;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User;

/// <summary>
/// Runs a whole live user connector through a relay the test can cut, and asserts it recovers; in the
/// <b>read</b> block, because it connects and reads and changes nothing.
/// </summary>
/// <remarks>
/// <para>
/// The counterpart of the other market type's relay test, and not a copy of it: recovery here also has to
/// fetch a new stream credential over the request API before a socket can be opened at all, so what is
/// measured is that chain plus everything above it — the sync cycle restarting, the loaders stopped and
/// started, state published again.
/// </para>
/// <para>
/// Only the account stream goes through the relay. The credential is fetched over the HTTP API, which
/// stays pointed at the venue, and the relay forwards the request line as the client sent it, so the
/// credential travelling in the path arrives untouched.
/// </para>
/// </remarks>
public class UserConnectorRelayTests : UserConnectorReadTestBase
{
    /// <summary>The port the relay will listen on, decided before the provider is configured.</summary>
    private readonly ushort _port = TestBaseWebSocketRelayExtensions.ReserveLocalPort();

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectorRelayTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public UserConnectorRelayTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the USD-M futures provider with its account stream pointed at the relay's address, and
    /// reloads quick enough that the snapshot a recovery republishes arrives inside the test's deadline.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceUsdFutures(
            new ProviderConfiguration
            {
                UserWsApi = new Uri($"ws://127.0.0.1:{_port}"),
                ReloadContext = new CompositeLoaderConfig(200, 5, 1000, 5_000, 100),
                ReloadOrders = new CompositeLoaderConfig(200, 5, 1000, 15_000, 100),
                ReloadTrades = new CompositeLoaderConfig(200, 5, 1000, 15_000, 100),
            }
        );
    }

    /// <summary>A cut connection brings the whole connector back, delivering again; in the <b>read</b> block.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.HoldTimeoutMs)]
    public async Task ACutConnection_BringsTheWholeConnectorBack()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var relay = this.RunWebSocketRelay(Endpoints.WsApi, _port);

        await UserConnectorDropBaseAsync(Settings.User, relay, ct);
    }
}
