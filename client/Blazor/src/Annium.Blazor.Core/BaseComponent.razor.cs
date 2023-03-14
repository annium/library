using System;
using System.Threading.Tasks;
using Annium.Components.State.Core;

namespace Annium.Blazor.Core;

public partial class BaseComponent : IAsyncDisposable
{
    protected AsyncDisposableBox Disposable = Annium.Disposable.AsyncBox();

    protected void ObserveStates() => Disposable += StateObserver.ObserveObject(this, StateHasChanged);

    public ValueTask DisposeAsync()
    {
        return Disposable.DisposeAsync();
    }
}