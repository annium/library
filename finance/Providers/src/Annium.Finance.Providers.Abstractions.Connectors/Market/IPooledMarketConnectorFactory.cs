using Annium.Finance.Providers.Abstractions.Domain.Market;

namespace Annium.Finance.Providers.Abstractions.Connectors.Market;

/// <summary>
/// Hands out leases on market connectors shared by settings, building a connector on the first lease and
/// tearing it down once the last one is returned.
/// </summary>
/// <remarks>
/// A provider meters what a connector costs it - request weight, open sockets - per account and per address,
/// not per connector, so callers that would each open their own connector for the same market are better off
/// sharing one. Depend on this factory rather than on <see cref="IMarketConnectorFactory"/> to say that the
/// connector is shared: the alternative, a second method on the plain factory, hides a different ownership
/// model behind a call site.
/// </remarks>
public interface IPooledMarketConnectorFactory
{
    /// <summary>
    /// Takes a lease on the connector shared by everything using these settings, building it on the first
    /// lease.
    /// </summary>
    /// <param name="settings">The market settings identifying the provider and market to connect to.</param>
    /// <returns>A lease on the shared market connector; disposing it gives the lease back.</returns>
    IMarketConnector Create(MarketSettings settings);
}
