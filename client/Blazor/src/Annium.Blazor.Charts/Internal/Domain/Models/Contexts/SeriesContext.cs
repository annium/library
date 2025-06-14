using System;
using System.Threading;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

/// <summary>
/// Provides context for rendering series data within a chart pane.
/// </summary>
internal sealed record SeriesContext : IManagedSeriesContext
{
    /// <summary>
    /// Gets the main canvas for rendering series data.
    /// </summary>
    public Canvas Canvas { get; private set; } = null!;

    /// <summary>
    /// Gets the overlay canvas for rendering interactive series elements.
    /// </summary>
    public Canvas Overlay { get; private set; } = null!;

    /// <summary>
    /// Gets the DOM rectangle representing the series area's bounds.
    /// </summary>
    public DomRect Rect { get; private set; }

    /// <summary>
    /// Flag indicating whether the context has been initiated.
    /// </summary>
    private int _isInitiated;

    /// <summary>
    /// Initializes the series context with canvas elements.
    /// </summary>
    /// <param name="canvas">The main canvas for rendering.</param>
    /// <param name="overlay">The overlay canvas for interactive elements.</param>
    public void Init(Canvas canvas, Canvas overlay)
    {
        if (Interlocked.CompareExchange(ref _isInitiated, 1, 0) != 0)
            throw new InvalidOperationException($"Can't init {nameof(VerticalSideContext)} more than once");

        Canvas = canvas;
        Overlay = overlay;
    }

    /// <summary>
    /// Sets the DOM rectangle bounds for the series area.
    /// </summary>
    /// <param name="rect">The DOM rectangle to set.</param>
    public void SetRect(DomRect rect)
    {
        Rect = rect;
    }
}
