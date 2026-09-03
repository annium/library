using System;
using Microsoft.AspNetCore.Components;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Blazor component that renders CSS styles to the page
/// </summary>
public partial class StyleSheet : IDisposable
{
    /// <summary>
    /// Gets or sets the internal stylesheet instance
    /// </summary>
    [Inject]
    internal Internal.StyleSheet Sheet { get; set; } = null!;

    /// <summary>
    /// Initializes the component by subscribing to CSS change events
    /// </summary>
    protected override void OnInitialized()
    {
        Sheet.CssChanged += StateHasChanged;
    }

    /// <summary>
    /// Disposes the component by unsubscribing from CSS change events
    /// </summary>
    public void Dispose()
    {
        Sheet.CssChanged -= StateHasChanged;
    }
}
