using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Annium.Blazor.Routing.Internal.Locations;

/// <summary>
/// Provides functionality for matching query string parameters against route requirements.
/// </summary>
internal interface ILocationQuery
{
    /// <summary>
    /// Attempts to match query string parameters against the expected query pattern.
    /// </summary>
    /// <param name="query">The query string parameters to match</param>
    /// <returns>The result of the matching operation including success status and extracted route values</returns>
    LocationMatch Match(IReadOnlyDictionary<string, StringValues> query);
}
