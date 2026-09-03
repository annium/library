using System;
using System.Threading;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

/// <summary>
/// Provides context for vertical side elements of the chart (left/right axes).
/// </summary>
internal sealed record VerticalSideContext : IManagedVerticalSideContext
{
    /// <summary>
    /// Gets the main canvas for rendering vertical side content.
    /// </summary>
    public Canvas Canvas { get; private set; } = null!;

    /// <summary>
    /// Gets the overlay canvas for rendering interactive vertical side elements.
    /// </summary>
    public Canvas Overlay { get; private set; } = null!;

    /// <summary>
    /// Gets the DOM rectangle representing the vertical side's bounds.
    /// </summary>
    public DomRect Rect { get; private set; }

    /// <summary>
    /// Flag indicating whether the context has been initiated.
    /// </summary>
    private int _isInitiated;

    /// <summary>
    /// Initializes the vertical side context with canvas elements.
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
    /// Sets the DOM rectangle bounds for the vertical side.
    /// </summary>
    /// <param name="rect">The DOM rectangle to set.</param>
    public void SetRect(DomRect rect)
    {
        Rect = rect;
    }
}
