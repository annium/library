using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.Shared;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.Market.Profiles;

/// <summary>
/// Mapper profile that resolves a <see cref="MarketConfig"/> (endpoints, websocket path) from the generic
/// <see cref="MarketSettings"/> for the USD-M futures provider.
/// </summary>
internal class MarketConfigProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarketConfigProfile"/> class, registering the
    /// <see cref="MarketSettings"/> to <see cref="MarketConfig"/> mapping.
    /// </summary>
    /// <param name="sp">The service provider used to resolve the shared <see cref="ProviderConfiguration"/>.</param>
    public MarketConfigProfile(IServiceProvider sp)
    {
        Map<MarketSettings, MarketConfig>(x => MapSettingsToConfig(sp, x));
    }

    /// <summary>
    /// Resolves the HTTP and websocket API endpoints for the settings' environment and fills in the fixed
    /// USD-M futures websocket stream path.
    /// </summary>
    /// <param name="sp">The service provider used to resolve the shared <see cref="ProviderConfiguration"/>.</param>
    /// <param name="settings">The generic market settings to map from.</param>
    /// <returns>The resolved market configuration.</returns>
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
