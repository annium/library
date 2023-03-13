using System;
using System.Threading.Tasks;

namespace Annium.Blazor.Core;

public partial class BaseComponent : IAsyncDisposable
{
    protected AsyncDisposableBox Disposable = Annium.Disposable.AsyncBox();

    protected void ObserveStates() => Disposable += ComponentBaseExtensions.ObserveStates(this);

    public ValueTask DisposeAsync()
    {
        return Disposable.DisposeAsync();
    }
}