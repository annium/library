using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Annium.Components.State.Core;

/// <summary>
/// Base implementation of observable state with change notification and muting capabilities.
/// </summary>
public abstract class ObservableState : IObservableState
{
    /// <summary>
    /// Gets an observable that emits a notification when the state changes.
    /// </summary>
    public IObservable<Unit> Changed { get; }
    private event Action StateChanged = delegate { };

    /// <summary>
    /// Indicates whether change notifications are currently muted.
    /// </summary>
    private bool _isMuted;

    protected ObservableState()
    {
        Changed = Observable.FromEvent(handle => StateChanged += handle, handle => StateChanged -= handle);
    }

    /// <summary>
    /// Temporarily mutes change notifications.
    /// </summary>
    /// <returns>A disposable that unmutes the state when disposed.</returns>
    public IDisposable Mute()
    {
        _isMuted = true;

        return new MuteScope(Unmute);
    }

    /// <summary>
    /// Notifies observers about state changes if not muted.
    /// </summary>
    protected void NotifyChanged()
    {
        if (!_isMuted)
            StateChanged.Invoke();
    }

    /// <summary>
    /// Unmutes change notifications.
    /// </summary>
    private void Unmute() => _isMuted = false;
}

/// <summary>
/// Represents a scope for temporarily muting observable state notifications.
/// </summary>
file readonly struct MuteScope : IDisposable
{
    /// <summary>
    /// The action to unmute notifications when the scope is disposed.
    /// </summary>
    private readonly Action _unmute;

    /// <summary>
    /// Initializes a new instance of the <see cref="MuteScope"/> struct.
    /// </summary>
    /// <param name="unmute">The action to invoke when disposing to unmute notifications.</param>
    public MuteScope(Action unmute)
    {
        _unmute = unmute;
    }

    /// <summary>
    /// Disposes the mute scope and restores change notifications.
    /// </summary>
    public void Dispose() => _unmute();
}
