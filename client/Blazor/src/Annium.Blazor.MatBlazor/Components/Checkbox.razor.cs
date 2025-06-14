using Annium.Components.State.Forms;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.MatBlazor.Components;

/// <summary>
/// A Material Design checkbox component that provides state management and customization options.
/// </summary>
public partial class Checkbox
{
    /// <summary>
    /// Gets or sets the atomic container that manages the checkbox state.
    /// </summary>
    [Parameter]
    public IAtomicContainer<bool> State { get; set; } = null!;

    /// <summary>
    /// Gets or sets the child content to be rendered within the checkbox label.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the checkbox component.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the checkbox is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }
}
