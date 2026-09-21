using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Logging;
using Annium.Net.Http;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User;

/// <summary>
/// Builds Binance spot <see cref="UserProvider"/> instances, resolving configuration, request signing and
/// the registered account/order/trade request factories.
/// </summary>
/// <param name="sp">The service provider used to resolve dependencies.</param>
internal class UserProviderFactory(IServiceProvider sp) : IUserProviderFactory
{
    /// <summary>Creates a new Binance spot user provider.</summary>
    /// <param name="settings">The account connection settings.</param>
    /// <returns>The created user provider.</returns>
    public IUserProvider Create(UserSettings settings)
    {
        var providerKey = settings.GetProviderKey();
        var config = sp.Resolve<IMapper>().Map<UserConfig>(settings);

        return new UserProvider(
            config,
            sp.CreateSignatureService(settings, providerKey),
            sp.ResolveHttpRequestFactory(Constants.GetAccountKey),
            sp.ResolveHttpRequestFactory(Constants.GetOrderKey),
            sp.ResolveHttpRequestFactory(Constants.GetTradeKey),
            sp.Resolve<IRateLimiter>(),
            sp.Resolve<ILogger>()
        );
    }
}
