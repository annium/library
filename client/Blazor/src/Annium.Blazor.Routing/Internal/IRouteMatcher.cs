using Annium.Blazor.Routing.Internal.Locations;

namespace Annium.Blazor.Routing.Internal;

/// <summary>
/// Provides functionality for matching raw locations against route patterns to determine routing data.
/// </summary>
internal interface IRouteMatcher
{
    /// <summary>
    /// Attempts to match a raw location against available routes using the specified path matching strategy.
    /// </summary>
    /// <param name="rawLocation">The raw location data to match</param>
    /// <param name="pathMatch">The path matching strategy to use</param>
    /// <returns>The matched location data if successful; otherwise, null</returns>
    LocationData? Match(RawLocation rawLocation, PathMatch pathMatch);
}
