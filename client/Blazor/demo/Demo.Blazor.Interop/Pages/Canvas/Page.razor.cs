using System;
using Annium;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Interop.Pages.Canvas;

/// <summary>
/// Canvas page component demonstrating the typed <see cref="Annium.Blazor.Interop.Canvas"/> wrapper by
/// drawing basic shapes and text after the first render.
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
    /// The canvas element used for drawing.
    /// </summary>
    private Annium.Blazor.Interop.Canvas _canvas = null!;

    /// <summary>
    /// Container for managing disposable resources.
    /// </summary>
    private DisposableBox _disposable = Disposable.Box(VoidLogger.Instance);

    /// <summary>
    /// Draws a filled rectangle, a stroked circle and a text label via the typed
    /// <see cref="Annium.Blazor.Interop.Canvas"/> wrapper once the element has been rendered.
    /// </summary>
    /// <param name="firstRender">True if this is the first render; otherwise false</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _disposable += _canvas;

        _canvas.FillStyle = "#4a90d9";
        _canvas.FillRect(10, 10, 120, 60);

        _canvas.StrokeStyle = "#333333";
        _canvas.LineWidth = 2;
        _canvas.BeginPath();
        _canvas.Arc(210, 45, 35, 0, (float)(2 * Math.PI), false);
        _canvas.Stroke();
        _canvas.ClosePath();

        _canvas.FillStyle = "#000000";
        _canvas.Font = "14px sans-serif";
        _canvas.FillText("Annium.Blazor.Interop Canvas", 10, 120);
    }

    /// <summary>
    /// Disposes of the page resources
    /// </summary>
    public void Dispose()
    {
        _disposable.Dispose();
    }
}
