using Demo.Blazor.Interop.Pages;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Interop.Components.Navigation;

public partial class Navigation
{
    [Inject]
    private Routing Routing { get; set; } = null!;

    [Inject]
    private Style Styles { get; set; } = null!;
}
