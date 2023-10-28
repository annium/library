using Annium.Components.State.Forms;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Ant.Pages.Home;

public partial class Page : ILogSubject
{
    [Inject]
    public IStateFactory StateFactory { get; set; } = default!;

    [Inject]
    public ILogger Logger { get; set; } = default!;

    private IObjectContainer<Data> _state = default!;

    protected override void OnInitialized()
    {
        _state = StateFactory.CreateObject(new Data());
        ObserveStates();
    }

    private void Toggle()
    {
        _state.AtAtomic(x => x.IsChecked).Set(!_state.AtAtomic(x => x.IsChecked).Value);
    }
}

public class Data
{
    public bool IsChecked { get; set; }
}
