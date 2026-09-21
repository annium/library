using System;
using System.Threading.Tasks;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.Shared;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Finance.Providers.Tests.Lib.User;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User;

/// <summary>
/// Runs a whole live user connector through a relay the test can cut, and asserts it recovers; in the
/// <b>read</b> block, because it connects and reads and changes nothing.
/// </summary>
/// <remarks>
/// <para>
/// The layer above <c>WsApiUserStreamLiveTests</c>. That one builds the account stream by hand and proves
/// the venue accepts a second subscription after a drop; everything between the stream and the caller —
/// the sync cycle restarting, the loaders stopped and started, state published again — it cannot see,
/// because nothing above the stream exists in it. Here the connector is built by its own factory, exactly
/// as production builds it, and the only thing that differs is where its stream points.
/// </para>
/// <para>
/// <b>Why this needed a feature rather than a test trick.</b> A connector's endpoints are resolved while
/// the container is built, and the relay cannot exist before the container it takes its services from. The
/// way out is to decide the address first: the test reserves a local port, configures the provider with it
/// through <see cref="ProviderConfiguration.UserWsApi"/>, and opens the relay on that port afterwards. That
/// setting is not a seam cut for testing — venues publish testnet endpoints, and a configurable endpoint is
/// what a caller needs to reach one.
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
    /// Registers the Binance spot provider with its account stream pointed at the relay's address, and
    /// reloads quick enough that the snapshot a recovery republishes arrives inside the test's deadline.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot(
            new ProviderConfiguration
            {
                // only the account stream is relayed: the HTTP API stays pointed at the venue, because the
                // connection worth cutting is the one that is supposed to survive being cut
                UserWsApi = new Uri($"ws://127.0.0.1:{_port}{Endpoints.UserWsApi.PathAndQuery}"),
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

        await using var relay = this.RunWebSocketRelay(Endpoints.UserWsApi, _port);

        await UserConnectorDropBaseAsync(Settings.User, relay, ct);
    }
}
