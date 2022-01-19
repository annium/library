using System.Collections.Generic;

namespace Annium.Blazor.Routing.Internal.Locations;

internal sealed record LocationMatch(bool IsSuccess, IReadOnlyDictionary<string, object?> RouteValues)
{
    public static LocationMatch Empty => new(false, new Dictionary<string, object?>());
}