using Annium.Finance.Providers.Core.Shared.Status;

namespace Annium.Finance.Providers.Core.Shared.TimeSync;

/// <summary>
/// Provides the provider's current server time, kept in sync in the background and extrapolated between
/// refreshes. One source serves every connector on that provider.
/// </summary>
public interface IServerTimeSource
{
    /// <summary>Gets the current server time, as Unix milliseconds, extrapolated from the last successful refresh.</summary>
    long ServerTime { get; }

    /// <summary>
    /// Gets the monitor carrying this source's own connection status and errors, for connectors to mirror
    /// into their aggregate.
    /// </summary>
    IStatusMonitor Monitor { get; }
}
