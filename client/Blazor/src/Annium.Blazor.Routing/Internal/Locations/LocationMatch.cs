using System.Collections.Generic;

namespace Annium.Blazor.Routing.Internal.Locations;

/// <summary>
/// Represents the result of a location matching operation, indicating success status and extracted route values.
/// </summary>
/// <param name="IsSuccess">Indicates whether the location matching was successful</param>
/// <param name="RouteValues">The route parameter values extracted during the matching process</param>
internal sealed record LocationMatch(bool IsSuccess, IReadOnlyDictionary<string, object?> RouteValues)
{
    /// <summary>
    /// Gets an empty location match representing a failed matching operation.
    /// </summary>
    public static LocationMatch Empty => new(false, new Dictionary<string, object?>());
}
