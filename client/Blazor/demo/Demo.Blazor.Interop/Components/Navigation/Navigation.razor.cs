using Demo.Blazor.Interop.Pages;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Interop.Components.Navigation;

/// <summary>
/// Navigation component for the Interop demo application
/// </summary>
public partial class Navigation
{
    /// <summary>
    /// Gets or sets the routing configuration
    /// </summary>
    [Inject]
    private Routing Routing { get; set; } = null!;

    /// <summary>
    /// Gets or sets the component styles
    /// </summary>
    [Inject]
    private Style Styles { get; set; } = null!;
}
