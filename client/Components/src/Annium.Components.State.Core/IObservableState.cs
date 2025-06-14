using System;
using System.Reactive;

namespace Annium.Components.State.Core;

/// <summary>
/// Defines an observable state that can notify about changes and can be muted.
/// </summary>
public interface IObservableState
{
    /// <summary>
    /// Gets an observable that emits a notification when the state changes.
    /// </summary>
    IObservable<Unit> Changed { get; }

    /// <summary>
    /// Temporarily mutes change notifications.
    /// </summary>
    /// <returns>A disposable that unmutes the state when disposed.</returns>
    IDisposable Mute();
}
