using System;
using System.Reactive.Subjects;
using System.Threading.Channels;
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
/// </remarks>
/// <typeparam name="T">The type of value carried.</typeparam>
internal class BufferedChannel<T> : IConnectorChannel<T>
{
    /// <summary>Gets the observable subscribers see the written values on.</summary>
    public IObservable<T> Observable => _subject;

    /// <summary>The subject the pump pushes values through, and the only thing subscribers hold.</summary>
    private readonly Subject<T> _subject = new();

    /// <summary>The writer side of the buffer, used by <see cref="Write"/>.</summary>
    private readonly ChannelWriter<T> _writer;

    /// <summary>The reader side of the buffer, drained by the pump <see cref="Connect"/> starts.</summary>
    private readonly ChannelReader<T> _reader;

    /// <summary>The logger the pump reports under.</summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferedChannel{T}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public BufferedChannel(ILogger logger)
    {
        var channel = Channel.CreateUnbounded<T>();
        _writer = channel.Writer;
        _reader = channel.Reader;
        _logger = logger;
    }

    /// <summary>Writes a value into the buffer.</summary>
    /// <param name="value">The value to write.</param>
    public void Write(T value) => _writer.Write(value);

    /// <summary>Starts the pump, which drains whatever is buffered and everything written after.</summary>
    /// <returns>A disposable that stops the pump, leaving later writes to accumulate again.</returns>
    public IAsyncDisposable Connect() => _reader.Pipe(_subject.OnNext, _logger);
}
