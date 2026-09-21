using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.Shared;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Internal.Market.Profiles;

/// <summary>Mapper profile that resolves a <see cref="MarketConfig"/> from generic <see cref="MarketSettings"/>.</summary>
internal class MarketConfigProfile : Profile
{
    /// <summary>Initializes a new instance of the <see cref="MarketConfigProfile"/> class, registering the settings-to-config mapping.</summary>
    /// <param name="sp">The service provider used to resolve the shared <see cref="ProviderConfiguration"/>.</param>
    public MarketConfigProfile(IServiceProvider sp)
    {
        Map<MarketSettings, MarketConfig>(x => MapSettingsToConfig(sp, x));
    }

    /// <summary>Maps generic market settings into a <see cref="MarketConfig"/>, resolving the Binance HTTP and WebSocket endpoints for the target environment.</summary>
    /// <param name="sp">The service provider used to resolve the shared <see cref="ProviderConfiguration"/>.</param>
    /// <param name="settings">The generic market settings to map from.</param>
    /// <returns>The resolved market config.</returns>
    private static MarketConfig MapSettingsToConfig(IServiceProvider sp, MarketSettings settings)
    {
        var providerConfig = sp.Resolve<ProviderConfiguration>();

        var httpApi = providerConfig.HttpApi ?? Endpoints.HttpApi;
        var wsApi = providerConfig.MarketWsApi ?? Endpoints.WsApi;

        return new MarketConfig
        {
            Provider = settings.Provider,
            HttpApi = httpApi,
            WsApi = wsApi,
            WsUriPath = Endpoints.MarketWsUriPath,
        };
    }
}
