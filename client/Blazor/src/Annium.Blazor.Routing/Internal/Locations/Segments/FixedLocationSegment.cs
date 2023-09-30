namespace Annium.Blazor.Routing.Internal.Locations.Segments;

internal sealed record FixedLocationSegment(string Part) : ILocationSegment
{
    public bool Match(string segment) => Part == segment;

    public override string ToString() => Part;
}