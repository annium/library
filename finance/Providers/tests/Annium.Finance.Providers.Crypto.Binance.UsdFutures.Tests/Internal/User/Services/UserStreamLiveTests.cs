using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.Shared;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User.Services;

/// <summary>
/// Drives the account stream against the real venue through a relay the test can cut; in the
/// <b>read</b> block, because it opens a stream and reads and changes nothing.
/// </summary>
/// <remarks>
/// <para>
/// The counterpart of the other venue's live drop test, and not a copy of it: recovery here goes through
/// a different mechanism. This venue's stream is opened on a key fetched over REST, and a dropped socket
/// makes the stream ask the resolver for a new one - so what is measured is the resolver and the stream
/// together, against a venue that has to agree to issue a second key and accept a second connection on
/// it. None of that is reachable offline, where the resolver is driven by hand.
/// </para>
/// <para>
/// A venue does not drop a connection to order, which is why the stream is pointed at a relay. The relay
/// forwards the request line as the client sent it, so the key in the path travels through untouched.
/// </para>
/// </remarks>
[Trait(TestBlock.Name, TestBlock.Read)]
public class UserStreamLiveTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserStreamLiveTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public UserStreamLiveTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the USD-M futures provider, which is what puts the signing, clock and listen key
    /// machinery this test uses into the container.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceUsdFutures();
    }

    /// <summary>
    /// A cut connection is reported, a new key is fetched, and the stream comes back — by the venue.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(
        Timeout = TestBlock.HoldTimeoutMs,
        Skip = "needs exchange credentials in test.env",
        SkipUnless = nameof(Exchange.HasCredentials),
        SkipType = typeof(Exchange)
    )]
    public async Task ACutConnection_FetchesANewKeyAndComesBack()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var sp = Get<IServiceProvider>();
        var monitor = new StatusMonitor(Logger);

        await using var relay = this.RunWebSocketRelay(Endpoints.WsApi);

        var settings = Settings.User;
        var signatureService = sp.CreateSignatureService(settings, settings.GetProviderKey());

        // the resolver talks HTTP to the venue directly - only the socket goes through the relay, because
        // the socket is the only thing the test needs to be able to cut
        var resolver = sp.CreateListenKeyResolver(
            Endpoints.HttpApi,
            new ListenKeyConfiguration(5_000, 60_000),
            Endpoints.ListenKeyUriPath,
            Constants.ListenKeyKey,
            signatureService,
            monitor
        );

        await using var stream = sp.CreateListenKeyUserStream(relay.Uri, Endpoints.UserWsUriPath, resolver, monitor);

        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
        var connectionsBeforeDrop = relay.AcceptedConnections;
        connectionsBeforeDrop.IsGreaterOrEqual(1, "the stream did not go through the relay at all");

        // act
        relay.Drop();

        // assert - it notices, which is the signal a hold test asserts the absence of
        await monitor.WaitStatusAsync(ConnectorStatus.Connecting, ct);

        // and comes back: on this venue that means the resolver asked for another key and the venue issued
        // one, then a second socket was opened on it - a chain the offline tests drive by hand and cannot
        // vouch for
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
        relay.AcceptedConnections.IsGreater(
            connectionsBeforeDrop,
            "the stream reported itself connected again without the relay seeing a new connection"
        );

        await resolver.DisposeAsync();
    }
}
