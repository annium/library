using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Domain.Contexts;

/// <summary>
/// Represents the context for vertical side components (left/right) of a chart
/// </summary>
public interface IVerticalSideContext
{
    /// <summary>
    /// Gets the main canvas for drawing vertical side content
    /// </summary>
    Canvas Canvas { get; }

    /// <summary>
    /// Gets the overlay canvas for drawing interactive elements
    /// </summary>
    Canvas Overlay { get; }

    /// <summary>
    /// Gets the DOM rectangle bounds of the vertical side area
    /// </summary>
    DomRect Rect { get; }
}
