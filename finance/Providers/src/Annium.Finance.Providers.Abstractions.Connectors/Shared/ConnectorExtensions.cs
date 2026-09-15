using System.Threading;
using System.Threading.Tasks;

namespace Annium.Finance.Providers.Abstractions.Connectors.Shared;

/// <summary>
/// Extension methods for <see cref="IConnectorBase"/>.
/// </summary>
public static class ConnectorExtensions
{
    /// <summary>
    /// Waits until the connector reaches the <see cref="ConnectorStatus.Connected"/> status. Completes
    /// immediately if the connector is already connected.
    /// </summary>
    /// <param name="connector">The connector to wait on.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the connector becomes connected.</returns>
    /// <remarks>
    /// A connector that never connects is an ordinary outcome - an exchange unreachable from where this
    /// runs, a network that drops the handshake - and without the token this wait had no end at all. A
    /// nightly run met exactly that and sat for 42 minutes until the runner killed it, which reports as
    /// cancelled rather than as failed and so reads like somebody pressed a button.
    ///
    /// The shape is the one <c>ClientSocketExtensions.WhenConnectedAsync</c> and its websocket twin
    /// already use, down to unsubscribing in a finally: this was the third of three and the only one
    /// without a token.
    /// </remarks>
    public static async Task WhenConnectedAsync(this IConnectorBase connector, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void HandleStatusChanged(ConnectorStatus status)
        {
            if (status is ConnectorStatus.Connected)
                tcs.TrySetResult();
        }

        // subscribe first, then look: reading the status first leaves a gap between the two that the
        // transition can fall into, seen by neither - and a caller that falls into it waits for an event
        // that has already happened, on a connector that is connected
        connector.OnStatusChanged += HandleStatusChanged;

        try
        {
            if (connector.Status is ConnectorStatus.Connected)
                return;

            await tcs.Task.WaitAsync(ct);
        }
        finally
        {
            // in a finally, so a cancelled wait does not leave the handler on the connector's
            // OnStatusChanged list - the handler no longer removes itself
            connector.OnStatusChanged -= HandleStatusChanged;
        }
    }
}
