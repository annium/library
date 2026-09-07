using System;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;

namespace Annium.Finance.Providers.Core.Shared.Status;

/// <summary>
/// Extension methods for wiring monitors together.
/// </summary>
public static class StatusMonitorExtensions
{
    /// <summary>
    /// Registers a component that reports through a monitor of its own as one target of this monitor, and
    /// keeps that target's status and errors in step with it.
    /// </summary>
    /// <remarks>
    /// This is how a connector counts a component it shares with other connectors - the provider's server
    /// time source above all. Such a component cannot report into any single connector's monitor, so it
    /// keeps its own and every connector mirrors it in for as long as that connector lives. Dispose the
    /// result to stop counting it.
    /// </remarks>
    /// <param name="monitor">The aggregate to register the component in.</param>
    /// <param name="source">The monitor the component reports its own status through.</param>
    /// <param name="component">The component to register, identified by its full id.</param>
    /// <returns>A handle that unmirrors the component and unregisters it.</returns>
    public static IDisposable Track(this IStatusMonitor monitor, IStatusMonitor source, object component)
    {
        var reporter = monitor.CreateReporter();
        reporter.Bind(component, source.Status);

        source.OnStatusChanged += HandleStatusChanged;
        source.OnError += reporter.Error;

        // the source runs on its own timer and may well have moved between the read above and the
        // subscription just made, which would leave the target holding a status that is already stale and
        // no further event to correct it. Reporting once more closes that window - and costs nothing when
        // nothing changed, since the monitor only raises on an actual transition
        HandleStatusChanged(source.Status);

        return Disposable.Create(() =>
        {
            source.OnStatusChanged -= HandleStatusChanged;
            source.OnError -= reporter.Error;
            reporter.Unbind();
        });

        void HandleStatusChanged(ConnectorStatus status)
        {
            switch (status)
            {
                case ConnectorStatus.Connected:
                    reporter.Connected();
                    break;
                case ConnectorStatus.Connecting:
                    reporter.Connecting();
                    break;
                default:
                    reporter.Disconnected();
                    break;
            }
        }
    }
}
