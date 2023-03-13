using System;
using System.Threading.Tasks;
using Annium.Components.State;
using Annium.Components.State.Core;

namespace Annium.Blazor.State;

public abstract class StateBase : ObservableState, IAsyncDisposable
{
    protected AsyncDisposableBox Disposable = Annium.Disposable.AsyncBox();

    public IDisposable ObserveStates() => StateObserver.ObserveObject(this, NotifyChanged);

    public ValueTask DisposeAsync()
    {
        return Disposable.DisposeAsync();
    }
}