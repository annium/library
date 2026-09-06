using Annium.Finance.Providers.Abstractions.Connectors.Market;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Core.Shared.Status;

namespace Annium.Finance.Providers.Core.Market;

/// <summary>
/// The seam a provider implements to build its own market connector: given the settings, the status monitor
/// the connector and its components report into, and the box their teardown is registered on.
/// </summary>
/// <remarks>
/// Both the monitor and the box are supplied by the caller rather than resolved, so a provider builds its
/// connector out of what it is handed and what the container holds for the whole process - it needs no DI
/// scope of its own, and callers that have a scope of their own can build connectors inside it.
/// </remarks>
public interface IMarketConnectorInstanceFactory
{
    /// <summary>
    /// Creates a market connector configured with the given settings.
    /// </summary>
    /// <param name="settings">The market settings identifying the provider and market to connect to.</param>
    /// <param name="monitor">The monitor this connector and every component it owns report their status into.</param>
    /// <param name="disposable">The disposable box the connector registers its resources with; disposing it tears the connector down.</param>
    /// <returns>A new market connector instance.</returns>
    IMarketConnector Create(MarketSettings settings, IStatusMonitor monitor, AsyncDisposableBox disposable);
}
