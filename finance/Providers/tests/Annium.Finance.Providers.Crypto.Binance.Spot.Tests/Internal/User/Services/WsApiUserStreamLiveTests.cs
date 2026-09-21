using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.Shared;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User.Services;

/// <summary>
/// Drives the account stream against the real venue through a relay the test can cut; in the
/// <b>read</b> block, because it subscribes and reads and changes nothing.
/// </summary>
/// <remarks>
/// <para>
/// The recovery path measured against the exchange rather than against our own test server. Offline
/// tests pin what the stream does when a socket drops - it resubscribes - and say nothing about what the
/// venue does when it comes back. A venue that refused a second subscription, rate limited one, or
/// required something new on it would show up here and in no other test.
/// </para>
/// <para>
/// It is also the positive control for the hold test. That one asserts the connector never left
/// connected, and an assertion that nothing happened cannot tell "it did not happen" from "nothing was
/// watching". Here the same signal is made to fire on purpose.
/// </para>
/// </remarks>
[Trait(TestBlock.Name, TestBlock.Read)]
public class WsApiUserStreamLiveTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WsApiUserStreamLiveTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public WsApiUserStreamLiveTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the Binance spot provider, which is what puts the signing and clock services the stream
    /// needs into the container.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot();
    }

    /// <summary>
    /// A cut connection is reported, re-established, and subscribed again — by the venue, not by a stub.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public async Task ACutConnection_ComesBackAndIsSubscribedAgain()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var sp = Get<IServiceProvider>();
        var monitor = new StatusMonitor(Get<ILogger>());

        await using var relay = this.RunWebSocketRelay(Endpoints.UserWsApi);

        var settings = Settings.User;
        var signatureService = sp.CreateSignatureService(settings, settings.GetProviderKey());

        await using var stream = sp.CreateWsApiUserStream(relay.Uri, 5_000, signatureService, monitor);

        // connected on this stream means the venue accepted a signed subscription, not merely that a
        // socket opened - so waiting for it is the assertion that the whole handshake works
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
        var connectionsBeforeDrop = relay.AcceptedConnections;
        connectionsBeforeDrop.IsGreaterOrEqual(1, "the stream did not go through the relay at all");

        // act
        relay.Drop();

        // assert - it notices, which is the signal the hold test asserts the absence of
        await monitor.WaitStatusAsync(ConnectorStatus.Connecting, ct);

        // and comes back subscribed, on a connection the relay saw being made
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
        relay.AcceptedConnections.IsGreater(
            connectionsBeforeDrop,
            "the stream reported itself subscribed again without the relay seeing a new connection"
        );
    }
}
