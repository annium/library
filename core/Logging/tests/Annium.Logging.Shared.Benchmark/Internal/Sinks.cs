using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Logging.Shared.Benchmark.Internal;

/// <summary>
/// Sink that accepts a batch and does nothing with it. Keeps a route's cost at the route's own cost —
/// the filter, the scheduler's dispatch shape — so what a benchmark measures is the pipeline rather than
/// whatever a real sink does with the batch.
/// </summary>
/// <remarks>
/// Deliberately not derived from <see cref="BufferingLogHandler{TContext}"/>: route selection picks the
/// background scheduler for a buffering handler and the immediate one for everything else, and the
/// benchmarks need both shapes from the same sink. The background route asks for its scheduler
/// explicitly, via <c>WithBackgroundScheduler()</c>.
/// </remarks>
internal sealed class NoOpSink : ILogHandler<DefaultLogContext>
{
    /// <summary>
    /// Completes without touching the batch.
    /// </summary>
    /// <param name="messages">The batch (ignored).</param>
    /// <param name="ct">Cancellation token (ignored).</param>
    /// <returns>A completed value task.</returns>
    public ValueTask HandleAsync(IReadOnlyList<LogMessage<DefaultLogContext>> messages, CancellationToken ct) =>
        ValueTask.CompletedTask;
}

/// <summary>
/// Sink that reads <see cref="LogMessage{TContext}.Data"/> the way a structured sink does, and does
/// nothing else with it. Paired with <see cref="NoOpSink"/>, the gap between the two is exactly what the
/// structured-data dictionary costs.
/// </summary>
/// <remarks>
/// Only two sinks in the library read <c>Data</c> — the Graylog and Seq ones, both of which enumerate it
/// into their wire format. Console, file, in-memory and xunit never touch it. So the gap this sink opens
/// is the tax an ordinary host pays for a dictionary nothing reads, and it must stay at zero for a host
/// whose sinks do not read it.
/// </remarks>
internal sealed class DataReadingSink : ILogHandler<DefaultLogContext>
{
    /// <summary>
    /// Number of key/value pairs read so far. Kept so the enumeration cannot be optimized away.
    /// </summary>
    public long PairsRead { get; private set; }

    /// <summary>
    /// Enumerates every message's structured data, counting the pairs.
    /// </summary>
    /// <param name="messages">The batch to read.</param>
    /// <param name="ct">Cancellation token (ignored).</param>
    /// <returns>A completed value task.</returns>
    public ValueTask HandleAsync(IReadOnlyList<LogMessage<DefaultLogContext>> messages, CancellationToken ct)
    {
        var read = PairsRead;

        for (var i = 0; i < messages.Count; i++)
            foreach (var _ in messages[i].Data)
                read++;

        PairsRead = read;

        return ValueTask.CompletedTask;
    }
}
