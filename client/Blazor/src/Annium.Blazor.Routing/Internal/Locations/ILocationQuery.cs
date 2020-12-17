using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Annium.Blazor.Routing.Internal.Locations
{
    internal interface ILocationQuery
    {
        LocationMatch Match(IReadOnlyDictionary<string, StringValues> query);
    }
}