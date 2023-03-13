using System;
using System.Threading.Tasks;
using Annium;
using Annium.Blazor.State;
using Annium.Blazor.State.Extensions;
using Annium.Components.State.Forms;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Ant.Pages.Home;

public partial class Page : ILogSubject<Page>, IAsyncDisposable
{
    [Inject]
    public IStateFactory StateFactory { get; set; } = default!;

    [Inject]
    public ILogger<Page> Logger { get; set; } = default!;

    private IObjectContainer<Data> _state = default!;
    private AsyncDisposableBox _disposable = Disposable.AsyncBox();

    protected override void OnInitialized()
    {
        _state = StateFactory.Create(new Data());
        _disposable += this.ObserveStates();
    }

    public ValueTask DisposeAsync()
    {
        this.Log().Debug("Dispose");
        return _disposable.DisposeAsync();
    }
}

public class Data
{
    public bool IsChecked { get; set; }
}