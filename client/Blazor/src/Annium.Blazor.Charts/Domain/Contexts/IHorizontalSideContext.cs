using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Domain.Contexts;

/// <summary>
/// Represents the context for horizontal side components (top/bottom) of a chart
/// </summary>
public interface IHorizontalSideContext
{
    /// <summary>
    /// Gets the main canvas for drawing horizontal side content
    /// </summary>
    Canvas Canvas { get; }

    /// <summary>
    /// Gets the overlay canvas for drawing interactive elements
    /// </summary>
    Canvas Overlay { get; }

    /// <summary>
    /// Gets the DOM rectangle bounds of the horizontal side area
    /// </summary>
    DomRect Rect { get; }
}
