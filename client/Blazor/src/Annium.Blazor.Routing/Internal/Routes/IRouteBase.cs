using System;
using Annium.Blazor.Routing.Internal.Locations;

namespace Annium.Blazor.Routing.Internal.Routes;

/// <summary>
/// Base interface for all route implementations providing core routing functionality.
/// </summary>
internal interface IRouteBase
{
    /// <summary>
    /// Gets the route template pattern used for matching.
    /// </summary>
    string Template { get; }

    /// <summary>
    /// Gets the page component type associated with this route.
    /// </summary>
    Type PageType { get; }

    /// <summary>
    /// Attempts to match a raw location against this route.
    /// </summary>
    /// <param name="raw">The raw location to match against.</param>
    /// <param name="match">The path matching strategy to use.</param>
    /// <returns>A location match result indicating success and any extracted route values.</returns>
    LocationMatch Match(RawLocation raw, PathMatch match);
}
