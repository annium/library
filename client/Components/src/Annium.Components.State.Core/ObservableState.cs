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

    /// <summary>Backing event that <see cref="Changed"/> is projected from; raised by <see cref="NotifyChanged"/>.</summary>
    private event Action StateChanged = delegate { };

    /// <summary>
    /// The number of currently-active mute scopes. Notifications are suppressed while this is greater than zero,
    /// so nested / overlapping <see cref="Mute"/> scopes on the same instance (e.g. an external batch wrapping
    /// internal Set/Init calls that mute themselves) compose correctly instead of unmuting early.
    /// </summary>
    private int _muteDepth;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableState"/> class, projecting
    /// <see cref="Changed"/> from the internal change event.
    /// </summary>
    protected ObservableState()
    {
        Changed = Observable.FromEvent(handle => StateChanged += handle, handle => StateChanged -= handle);
    }

    /// <summary>
    /// Temporarily mutes change notifications. Mute scopes nest: notifications resume only when the outermost
    /// scope is disposed.
    /// </summary>
    /// <returns>A disposable that unmutes the state when disposed.</returns>
    public IDisposable Mute()
    {
        _muteDepth++;

        return new MuteScope(Unmute);
    }

    /// <summary>
    /// Notifies observers about state changes if not muted.
    /// </summary>
    protected void NotifyChanged()
    {
        if (_muteDepth == 0)
            StateChanged.Invoke();
    }

    /// <summary>
    /// Ends one mute scope, restoring notifications once all scopes are disposed.
    /// </summary>
    private void Unmute() => _muteDepth--;
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
