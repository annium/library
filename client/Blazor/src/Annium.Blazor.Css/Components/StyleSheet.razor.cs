using System;
using Microsoft.AspNetCore.Components;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

public partial class StyleSheet : IDisposable
{
    [Inject]
    internal Internal.StyleSheet Sheet { get; set; } = default!;

    protected override void OnInitialized()
    {
        Sheet.CssChanged += StateHasChanged;
    }

    public void Dispose()
    {
        Sheet.CssChanged -= StateHasChanged;
    }
}