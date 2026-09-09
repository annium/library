using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Annium.Finance.Providers.Core.Internal.Shared.Channels;

/// <summary>
/// Delivers each written value to the subscribers on the thread that wrote it, before the write returns.
/// </summary>
/// <remarks>
/// The counterpart of <see cref="BufferedChannel{T}"/> for a connector that produces its own values on the
/// caller's thread. Values written before <see cref="Connect"/> are held and delivered when it is called,
/// matching what the channel pair does with its source channel - a connector writes during construction,
/// and the sync cycle that connects it comes later.
/// </remarks>
/// <typeparam name="T">The type of value carried.</typeparam>
internal sealed class InlineChannel<T> : IConnectorChannel<T>
{
    /// <summary>Gets the observable subscribers see the written values on.</summary>
    public IObservable<T> Observable => _subject;

    /// <summary>The multicast subject values are pushed through, synchronously.</summary>
    private readonly Subject<T> _subject = new();

    /// <summary>Values written while disconnected, delivered on the next <see cref="Connect"/>.</summary>
    private readonly List<T> _held = new();

    /// <summary>Whether writes currently reach the subscribers.</summary>
    private bool _isConnected;

    /// <summary>Writes a value, delivering it to the subscribers if connected, holding it if not.</summary>
    /// <param name="value">The value to write.</param>
    public void Write(T value)
    {
        if (!_isConnected)
        {
            _held.Add(value);
            return;
        }

        _subject.OnNext(value);
    }

    /// <summary>Starts delivering, flushing whatever was held first.</summary>
    /// <returns>A disposable that stops delivery.</returns>
    public IAsyncDisposable Connect()
    {
        _isConnected = true;

        if (_held.Count > 0)
        {
            foreach (var value in _held)
                _subject.OnNext(value);
            _held.Clear();
        }

        return new Disconnect(this);
    }

    /// <summary>Stops delivery when the sync cycle that connected the channel is torn down.</summary>
    /// <param name="channel">The channel to disconnect.</param>
    private sealed class Disconnect(InlineChannel<T> channel) : IAsyncDisposable
    {
        /// <summary>Marks the channel disconnected, so later writes are held again rather than delivered.</summary>
        /// <returns>A completed task.</returns>
        public ValueTask DisposeAsync()
        {
            channel._isConnected = false;

            return ValueTask.CompletedTask;
        }
    }
}
