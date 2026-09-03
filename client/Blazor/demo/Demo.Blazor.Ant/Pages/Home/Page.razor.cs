using Annium.Components.State.Forms;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Ant.Pages.Home;

/// <summary>
/// Home page component for the Demo.Blazor.Ant application demonstrating state management.
/// </summary>
public partial class Page : ILogSubject
{
    /// <summary>
    /// Gets or sets the state factory for creating object containers.
    /// </summary>
    [Inject]
    public IStateFactory StateFactory { get; set; } = null!;

    /// <summary>
    /// Gets or sets the logger instance for this component.
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// The state container for the page data.
    /// </summary>
    private IObjectContainer<Data> _state = null!;

    /// <summary>
    /// Initializes the component by creating the state container and observing state changes.
    /// </summary>
    protected override void OnInitialized()
    {
        _state = StateFactory.CreateObject(new Data());
        ObserveStates();
    }

    /// <summary>
    /// Toggles the IsChecked state value.
    /// </summary>
    private void Toggle()
    {
        _state.AtAtomic(x => x.IsChecked).Set(!_state.AtAtomic(x => x.IsChecked).Value);
    }
}

/// <summary>
/// Data model for the home page state.
/// </summary>
public class Data
{
    /// <summary>
    /// Gets or sets a value indicating whether the checkbox is checked.
    /// </summary>
    public bool IsChecked { get; set; }
}
