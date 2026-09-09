using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using Annium.Finance.Providers.Core.Shared;
using Annium.Logging;
using Annium.Threading.Channels;

namespace Annium.Finance.Providers.Core.Internal.Shared.Channels;

/// <summary>
/// Buffers written values in a channel and delivers them to the subscribers from a background pump.
/// </summary>
/// <remarks>
/// What a live connector needs: its values arrive from a socket, on a thread it does not own and must not
/// block, so the write has to be a handoff and the channel is where the handoff lands. Values written
/// before <see cref="Connect"/> wait in the channel and go out when the pump starts, which is how a
/// connector can write during its own construction and have the sync cycle connect it later.
///
/// A buffer that no consumer keeps up with is a question this class answers rather than avoids — see
/// <see cref="ConnectorBuffer"/>. Either way the answer is reported: a queue nobody watches turns a slow
/// consumer into a memory leak or a silent gap, and both are worse than a log line.
/// </remarks>
/// <typeparam name="T">The type of value carried.</typeparam>
internal sealed class BufferedChannel<T> : IConnectorChannel<T>, ILogSubject
{
    /// <summary>
    /// How many values a <see cref="ConnectorBuffer.Recent"/> buffer holds before it starts dropping.
    /// </summary>
    /// <remarks>
    /// Deep enough that a consumer pausing briefly loses nothing, shallow enough that what survives a real
    /// stall is recent. At a busy symbol's update rate this is on the order of a second of ticks.
    /// </remarks>
    private const int RecentCapacity = 1024;

    /// <summary>
    /// How deep a <see cref="ConnectorBuffer.Complete"/> buffer gets before the consumer is called behind.
    /// </summary>
    private const int DepthThreshold = 10_000;

    /// <summary>How many drops pass between reports, after the first one.</summary>
    private const int DropReportInterval = 1_000;

    /// <summary>Gets the logger.</summary>
    public ILogger Logger { get; }

    /// <summary>Gets the observable subscribers see the written values on.</summary>
    public IObservable<T> Observable => _subject;

    /// <summary>The subject the pump pushes values through, and the only thing subscribers hold.</summary>
    private readonly Subject<T> _subject = new();

    /// <summary>The writer side of the buffer, used by <see cref="Write"/>.</summary>
    private readonly ChannelWriter<T> _writer;

    /// <summary>The reader side of the buffer, drained by the pump <see cref="Connect"/> starts.</summary>
    private readonly ChannelReader<T> _reader;

    /// <summary>Total values dropped, for a <see cref="ConnectorBuffer.Recent"/> buffer.</summary>
    private long _dropped;

    /// <summary>Whether the queue is currently past <see cref="DepthThreshold"/>, so the crossing is reported once.</summary>
    private bool _isDeep;

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferedChannel{T}"/> class.
    /// </summary>
    /// <param name="buffer">What the buffer owes values written while the consumer is behind.</param>
    /// <param name="logger">The logger instance.</param>
    public BufferedChannel(ConnectorBuffer buffer, ILogger logger)
    {
        Logger = logger;

        var channel =
            buffer is ConnectorBuffer.Recent
                ? Channel.CreateBounded<T>(
                    new BoundedChannelOptions(RecentCapacity) { FullMode = BoundedChannelFullMode.DropOldest },
                    ReportDropped
                )
                : Channel.CreateUnbounded<T>();

        _writer = channel.Writer;
        _reader = channel.Reader;
    }

    /// <summary>Writes a value into the buffer.</summary>
    /// <remarks>
    /// Never blocks and never fails: a bounded buffer drops its oldest value to make room, an unbounded one
    /// grows. Both say so.
    /// </remarks>
    /// <param name="value">The value to write.</param>
    public void Write(T value)
    {
        _writer.TryWrite(value);

        WatchDepth();
    }

    /// <summary>Starts the pump, which drains whatever is buffered and everything written after.</summary>
    /// <returns>A disposable that stops the pump, leaving later writes to accumulate again.</returns>
    public IAsyncDisposable Connect() => _reader.Pipe(_subject.OnNext, Logger);

    /// <summary>
    /// Reports a value the bounded buffer dropped to make room.
    /// </summary>
    /// <remarks>
    /// The first one is reported on its own, because the moment dropping starts is the interesting one;
    /// after that one report per <see cref="DropReportInterval"/> keeps the loss visible without the log
    /// itself becoming the load.
    /// </remarks>
    /// <param name="_">The dropped value; the count is what matters, not which one went.</param>
    private void ReportDropped(T _)
    {
        var dropped = Interlocked.Increment(ref _dropped);

        if (dropped == 1 || dropped % DropReportInterval == 0)
            this.Warn(
                "buffer of {capacity} is full - dropped {dropped} value(s) so far; the subscribers are behind",
                RecentCapacity,
                dropped
            );
    }

    /// <summary>
    /// Reports the unbounded buffer crossing <see cref="DepthThreshold"/>, and coming back under it.
    /// </summary>
    /// <remarks>
    /// Once each way, not per value: what matters is that a consumer fell behind and whether it caught up.
    /// </remarks>
    private void WatchDepth()
    {
        if (!_reader.CanCount)
            return;

        var count = _reader.Count;

        if (!_isDeep && count > DepthThreshold)
        {
            _isDeep = true;
            this.Warn("buffer is {count} deep - the subscribers are behind", count);
        }
        else if (_isDeep && count <= DepthThreshold / 2)
        {
            _isDeep = false;
            this.Warn("buffer is back down to {count} - the subscribers have caught up", count);
        }
    }
}
