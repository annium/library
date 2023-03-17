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
}

public class Data
{
    public bool IsChecked { get; set; }
}