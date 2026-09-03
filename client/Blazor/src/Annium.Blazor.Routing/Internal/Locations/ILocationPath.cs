using System.Collections.Generic;

namespace Annium.Blazor.Routing.Internal.Locations;

/// <summary>
/// Provides functionality for matching and generating URL paths for routing.
/// </summary>
internal interface ILocationPath
{
    /// <summary>
    /// Attempts to match URL segments against the path pattern using the specified matching strategy.
    /// </summary>
    /// <param name="segments">The URL segments to match</param>
    /// <param name="match">The path matching strategy to use</param>
    /// <returns>The result of the matching operation including success status and extracted route values</returns>
    LocationMatch Match(IReadOnlyList<string> segments, PathMatch match);

    /// <summary>
    /// Generates a URL link from the path pattern using the provided parameters.
    /// </summary>
    /// <param name="parameters">The parameters to substitute into the path pattern</param>
    /// <returns>The generated URL string</returns>
    string Link(IReadOnlyDictionary<string, object?> parameters);
}
