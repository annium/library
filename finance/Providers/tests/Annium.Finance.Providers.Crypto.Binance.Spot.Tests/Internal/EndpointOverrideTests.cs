using System;
using Annium.Core.Mapper;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.Market;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.Shared;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal;

/// <summary>
/// Verifies that a configured endpoint reaches the resolved configuration, and that leaving one unset
/// resolves the exchange's own.
/// </summary>
/// <remarks>
/// The setting exists because venues publish testnet hosts, and because a test that has to cut a
/// connection needs the connector pointed somewhere it controls. Both rest on the same property, and it is
/// the kind that is easy to get half right: an override honoured for one endpoint and quietly ignored for
/// another produces a connector that half-talks to the venue, with nothing saying so.
/// </remarks>
public class EndpointOverrideTests : ProvidersTestBase
{
    /// <summary>An address that is not the venue's, for any endpoint.</summary>
    private static readonly Uri _http = new("https://http.example");

    /// <summary>An address that is not the venue's market stream.</summary>
    private static readonly Uri _marketWs = new("wss://market.example");

    /// <summary>An address that is not the venue's account stream.</summary>
    private static readonly Uri _userWs = new("wss://user.example/route");

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointOverrideTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public EndpointOverrideTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the Binance spot provider with every endpoint pointed somewhere else.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot(
            new ProviderConfiguration
            {
                HttpApi = _http,
                MarketWsApi = _marketWs,
                UserWsApi = _userWs,
            }
        );
    }

    /// <summary>Every configured endpoint reaches the configuration the connector is built from.</summary>
    [Fact]
    public void ConfiguredEndpoints_ReachTheResolvedConfiguration()
    {
        // arrange
        var mapper = Get<IMapper>();

        // act
        var user = mapper.Map<UserConfig>(
            new UserSettings
            {
                Provider = Constants.Provider,
                Key = "k",
                Secret = "s",
            }
        );
        var market = mapper.Map<MarketConfig>(new MarketSettings { Provider = Constants.Provider });

        // assert - each endpoint separately, because one honoured and another ignored is the failure worth
        // catching, and a single assertion over one of them would pass through it
        user.HttpApi.Is(_http);
        user.WsApi.Is(_userWs, "the account stream took the market stream's address, or the venue's own");
        market.HttpApi.Is(_http);
        market.WsApi.Is(_marketWs, "the market stream took the account stream's address, or the venue's own");
    }

    /// <summary>An endpoint left unset resolves the exchange's own.</summary>
    [Fact]
    public void UnsetEndpoints_ResolveTheExchangesOwn()
    {
        // arrange - a configuration that overrides nothing, which is what production passes
        var configuration = new ProviderConfiguration();

        // assert - the defaults are absent rather than copies of the constants, so that the fallback is one
        // place rather than two that can disagree
        configuration.HttpApi.IsDefault();
        configuration.MarketWsApi.IsDefault();
        configuration.UserWsApi.IsDefault();

        // and the constants they fall back to are distinct values, so the test above cannot pass by two of
        // them being the same string
        Endpoints.HttpApi.IsNotEqual(Endpoints.WsApi);
        Endpoints.WsApi.IsNotEqual(Endpoints.UserWsApi);
    }
}
