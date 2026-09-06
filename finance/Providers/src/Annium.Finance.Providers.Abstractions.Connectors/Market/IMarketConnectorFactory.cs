using Annium.Finance.Providers.Abstractions.Domain.Market;

namespace Annium.Finance.Providers.Abstractions.Connectors.Market;

/// <summary>
/// Creates <see cref="IMarketConnector"/> instances, resolving all their dependencies through the container
/// (used to build standalone connectors, e.g. registered as singletons in DI).
/// </summary>
public interface IMarketConnectorFactory
{
    /// <summary>
    /// Creates a market connector configured with the given settings.
    /// </summary>
    /// <param name="settings">The market settings identifying the provider and market to connect to.</param>
    /// <returns>A new market connector instance the caller owns; disposing it tears it down.</returns>
    IMarketConnector Create(MarketSettings settings);

    /// <summary>
    /// Takes a lease on the connector shared by everything using these settings, building it on the
    /// first lease.
    /// </summary>
    /// <remarks>
    /// A provider charges its rate limit per account rather than per connector, so callers that each
    /// want a connector for the same settings - one per instrument, say - should share one instead of
    /// opening their own. Disposing the returned value gives the lease back; the connector itself is
    /// torn down once the last lease is returned.
    /// </remarks>
    /// <param name="settings">The market settings identifying the provider and market to connect to.</param>
    /// <returns>A lease on the shared market connector.</returns>
    IMarketConnector CreatePooled(MarketSettings settings);
}
