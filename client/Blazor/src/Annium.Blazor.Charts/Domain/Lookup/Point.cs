namespace Annium.Blazor.Charts.Domain.Lookup;

/// <summary>
/// Represents a point with X and Y coordinates.
/// </summary>
/// <param name="X">The X coordinate of the point.</param>
/// <param name="Y">The Y coordinate of the point.</param>
public readonly record struct Point(int X, int Y);
