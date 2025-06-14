using System;
using System.Threading.Tasks;
using Annium.Components.State.Core;
using Annium.Logging;

namespace Annium.Blazor.Core;

/// <summary>
/// Base component class that provides common functionality for Blazor components including state observation and async disposal.
/// </summary>
public partial class BaseComponent : IAsyncDisposable
{
    /// <summary>
    /// Container for managing async disposable resources used by the component.
    /// </summary>
    protected AsyncDisposableBox Disposable = Annium.Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// Observes state changes on this component and triggers re-rendering when state changes occur.
    /// </summary>
    protected void ObserveStates() => Disposable += StateObserver.ObserveObject(this, StateHasChanged);

    /// <summary>
    /// Asynchronously disposes of the component and all its managed resources.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous disposal operation.</returns>
    public ValueTask DisposeAsync()
    {
        return Disposable.DisposeAsync();
    }
}
