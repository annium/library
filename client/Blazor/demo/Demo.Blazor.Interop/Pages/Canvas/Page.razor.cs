using System;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Interop.Pages.Canvas;

/// <summary>
/// Canvas page component for demonstrating JavaScript interop functionality
/// </summary>
public partial class Page : ILogSubject, IDisposable
{
    /// <summary>
    /// Gets or sets the page styles
    /// </summary>
    [Inject]
    private Style Styles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the logger instance
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Called after the component is rendered
    /// </summary>
    /// <param name="firstRender">True if this is the first render; otherwise false</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        Console.WriteLine("Canvas manipulation here");
    }

    /// <summary>
    /// Disposes of the page resources
    /// </summary>
    public void Dispose()
    {
        Console.WriteLine("Canvas page disposed");
    }
}
