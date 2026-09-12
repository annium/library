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
