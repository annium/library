using System.Threading;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Core.Shared.Status;

namespace Annium.Finance.Providers.Tests.Lib.Infrastructure;

/// <summary>
/// Lets a test wait on what a component reports through an <see cref="IStatusMonitor"/>.
/// </summary>
public static class StatusMonitorTestExtensions
{
    /// <summary>
    /// Waits until the monitor reports the given status.
    /// </summary>
    /// <remarks>
    /// Call it before the act rather than after it, and keep the returned task: a status a component passes
    /// through - connecting, on its way back to connected after a drop - is gone by the time anything polls
    /// for it. The subscription is taken before the current status is read, so neither ordering loses it.
    /// </remarks>
    /// <param name="monitor">The monitor to watch.</param>
    /// <param name="target">The status to wait for.</param>
    /// <param name="ct">The test's cancellation token, so a status that never arrives ends with the test.</param>
    /// <returns>A task that completes once the monitor reports the status.</returns>
    public static async Task WaitStatusAsync(this IStatusMonitor monitor, ConnectorStatus target, CancellationToken ct)
    {
        var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void Handle(ConnectorStatus status)
        {
            if (status == target)
                reached.TrySetResult();
        }

        monitor.OnStatusChanged += Handle;
        try
        {
            if (monitor.Status == target)
                reached.TrySetResult();

            await reached.Task.WaitAsync(ct);
        }
        finally
        {
            monitor.OnStatusChanged -= Handle;
        }
    }
}
