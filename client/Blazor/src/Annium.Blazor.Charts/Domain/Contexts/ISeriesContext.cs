using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Domain.Contexts;

/// <summary>
/// Represents the context for drawing series data within a chart pane
/// </summary>
public interface ISeriesContext
{
    /// <summary>
    /// Gets the main canvas for drawing series content
    /// </summary>
    Canvas Canvas { get; }

    /// <summary>
    /// Gets the overlay canvas for drawing interactive series elements
    /// </summary>
    Canvas Overlay { get; }

    /// <summary>
    /// Gets the DOM rectangle bounds of the series area
    /// </summary>
    DomRect Rect { get; }
}
