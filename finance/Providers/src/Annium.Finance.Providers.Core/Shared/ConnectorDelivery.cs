namespace Annium.Finance.Providers.Core.Shared;

/// <summary>
/// How a connector hands the values it produces to the subscribers of its observables.
/// </summary>
public enum ConnectorDelivery
{
    /// <summary>
    /// Values are written into a channel and delivered from a background pump.
    /// </summary>
    /// <remarks>
    /// What a live connector needs: its values arrive from a socket, on a thread it does not own and must
    /// not block, so writing has to be a handoff. This is the default.
    /// </remarks>
    Buffered = 0,

    /// <summary>
    /// Values are delivered to subscribers on the thread that wrote them, before the write returns.
    /// </summary>
    /// <remarks>
    /// For a connector that produces its values itself, on the caller's thread, from something it already
    /// holds - a replay of stored data. The handoff buys such a connector nothing and costs it a thread
    /// transition per value, plus the need to discover, somehow, when delivery has finished. Inline
    /// delivery makes "it has been delivered" the same moment as "write returned".
    ///
    /// A subscriber that blocks the writer. That is the point, and it is only safe because the
    /// writer is the one driving the run.
    /// </remarks>
    Inline = 1,
}
