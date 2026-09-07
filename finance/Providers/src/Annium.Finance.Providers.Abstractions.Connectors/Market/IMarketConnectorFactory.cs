using Annium.Finance.Providers.Abstractions.Domain.Market;

namespace Annium.Finance.Providers.Abstractions.Connectors.Market;

/// <summary>
/// Creates standalone <see cref="IMarketConnector"/> instances, each owning everything it is built from.
/// </summary>
/// <remarks>
/// Resolve this from the scope the connector belongs to: the factory builds in whatever provider it was
/// resolved through, so a caller that runs work in its own scope gets connectors wired to that scope's
/// services, and a caller with no scope of its own gets the container's.
/// </remarks>
public interface IMarketConnectorFactory
{
    /// <summary>
    /// Creates a market connector configured with the given settings.
    /// </summary>
    /// <param name="settings">The market settings identifying the provider and market to connect to.</param>
    /// <returns>A new market connector instance the caller owns; disposing it tears it down.</returns>
    IMarketConnector Create(MarketSettings settings);
}
