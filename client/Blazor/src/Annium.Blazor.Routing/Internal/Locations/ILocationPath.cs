using System.Collections.Generic;

namespace Annium.Blazor.Routing.Internal.Locations;

internal interface ILocationPath
{
    LocationMatch Match(IReadOnlyList<string> segments, PathMatch match);
    string Link(IReadOnlyDictionary<string, object?> parameters);
}