// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

/// <summary>
/// Represents a DOM rectangle with position and dimensions.
/// </summary>
public readonly record struct DomRect
{
    /// <summary>
    /// Gets the X coordinate of the rectangle's left edge.
    /// </summary>
    public decimal X { get; init; }

    /// <summary>
    /// Gets the Y coordinate of the rectangle's top edge.
    /// </summary>
    public decimal Y { get; init; }

    /// <summary>
    /// Gets the width of the rectangle.
    /// </summary>
    public decimal Width { get; init; }

    /// <summary>
    /// Gets the height of the rectangle.
    /// </summary>
    public decimal Height { get; init; }
}
