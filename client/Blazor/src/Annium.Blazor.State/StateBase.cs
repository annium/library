using System;
using System.Threading.Tasks;
using Annium.Components.State.Core;
using Annium.Logging;

namespace Annium.Blazor.State;

public abstract class StateBase : ObservableState, IAsyncDisposable
{
    protected AsyncDisposableBox Disposable = Annium.Disposable.AsyncBox(VoidLogger.Instance);

    protected void ObserveStates() => Disposable += StateObserver.ObserveObject(this, NotifyChanged);

    public ValueTask DisposeAsync()
    {
        return Disposable.DisposeAsync();
    }
}