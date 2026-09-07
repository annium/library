using System;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;

namespace Annium.Finance.Providers.Core.Shared.Status;

/// <summary>
/// Aggregates the connection status of a set of named targets into a single overall <see cref="Status"/>:
/// connected only if every target is connected, disconnected only if every target is disconnected, and
/// connecting otherwise. A target joins by taking a reporter from <see cref="CreateReporter"/> and binding
/// itself to it.
/// </summary>
/// <remarks>
/// One monitor covers one connector and the components it is built from - its provider, sockets, loaders and
/// the server time source it reads. That grouping used to be expressed as a DI scope per connector, which
/// made every scoped registration a per-connector one by accident; the monitor is now created by the
/// connector factory and handed down explicitly, so it is the only thing per-connector by construction.
/// </remarks>
public interface IStatusMonitor
{
    /// <summary>Gets the overall connection status resolved from all registered targets.</summary>
    ConnectorStatus Status { get; }

    /// <summary>Raised whenever <see cref="Status"/> changes, with the new status.</summary>
    event Action<ConnectorStatus> OnStatusChanged;

    /// <summary>Surfaces errors reported by registered targets.</summary>
    event Action<ConnectorError> OnError;

    /// <summary>
    /// Creates a reporter through which one more component can join this monitor's aggregate status.
    /// </summary>
    /// <returns>A reporter that is not bound to any component yet.</returns>
    IStatusReporter CreateReporter();
}
