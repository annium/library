using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Connectors.Market;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Core.Internal.Market;
using Annium.Finance.Providers.Core.Internal.User;

namespace Annium.Finance.Providers.Core;

/// <summary>
/// Entry point extension methods for registering finance providers into an <see cref="IServiceContainer"/>.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the shared services every finance provider depends on (plain and pooling connector
    /// factories), and returns a context for registering individual providers via
    /// <see cref="ProviderRegistrationContext.AddProvider{TMarketProviderFactory, TMarketConnectorFactory, TUserProviderFactory, TUserConnectorFactory, TFinanceService}"/>.
    /// </summary>
    /// <param name="container">The container to register services into.</param>
    /// <returns>A registration context bound to <paramref name="container"/>.</returns>
    public static ProviderRegistrationContext AddFinanceProviders(this IServiceContainer container)
    {
        // market
        container.Add<IMarketConnectorFactory, MarketConnectorFactory>().Transient();
        container.Add<IPooledMarketConnectorFactory, PooledMarketConnectorFactory>().Singleton();

        // user
        container.Add<IUserConnectorFactory, UserConnectorFactory>().Transient();
        container.Add<IPooledUserConnectorFactory, PooledUserConnectorFactory>().Singleton();

        var ctx = new ProviderRegistrationContext(container);

        return ctx;
    }
}
