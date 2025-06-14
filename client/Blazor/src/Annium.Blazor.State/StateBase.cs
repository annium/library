using System;
using System.Threading.Tasks;
using Annium.Components.State.Core;
using Annium.Logging;

namespace Annium.Blazor.State;

/// <summary>
/// Base class for Blazor state management that provides observable state functionality and async disposal.
/// </summary>
public abstract class StateBase : ObservableState, IAsyncDisposable
{
    /// <summary>
    /// Container for managing async disposable resources associated with this state.
    /// </summary>
    protected AsyncDisposableBox Disposable = Annium.Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// Sets up state observation to automatically notify when state changes occur.
    /// </summary>
    protected void ObserveStates() => Disposable += StateObserver.ObserveObject(this, NotifyChanged);

    /// <summary>
    /// Asynchronously disposes of all resources managed by this state.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous disposal operation.</returns>
    public ValueTask DisposeAsync()
    {
        return Disposable.DisposeAsync();
    }
}
