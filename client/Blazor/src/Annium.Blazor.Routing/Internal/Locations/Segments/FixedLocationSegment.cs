namespace Annium.Blazor.Routing.Internal.Locations.Segments;

/// <summary>
/// Represents a fixed location segment that matches a literal string value
/// </summary>
/// <param name="Part">The literal string part that this segment represents</param>
internal sealed record FixedLocationSegment(string Part) : ILocationSegment
{
    /// <summary>
    /// Determines whether the provided segment matches this fixed segment
    /// </summary>
    /// <param name="segment">The segment to match</param>
    /// <returns>True if the segment matches exactly; otherwise, false</returns>
    public bool Match(string segment) => Part == segment;

    /// <summary>
    /// Returns the string representation of this fixed location segment
    /// </summary>
    /// <returns>The literal part string</returns>
    public override string ToString() => Part;
}
