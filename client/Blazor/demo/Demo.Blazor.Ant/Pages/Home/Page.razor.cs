using Annium.Components.State.Forms;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Ant.Pages.Home;

public partial class Page : ILogSubject<Page>
{
    [Inject]
    public IStateFactory StateFactory { get; set; } = default!;

    [Inject]
    public ILogger<Page> Logger { get; set; } = default!;

    private IObjectContainer<Data> _state = default!;

    protected override void OnInitialized()
    {
        _state = StateFactory.CreateObject(new Data());
        ObserveStates();
    }

    private void Toggle()
    {
        _state.At(x => x.IsChecked).Set(!_state.At(x => x.IsChecked).Value);
    }
}

public class Data
{
    public bool IsChecked { get; set; }
}