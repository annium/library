using System;
using System.Collections.Generic;
using Annium.Blazor.Routing.Internal.Locations;
using Annium.Core.Mapper;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Routing.Internal.Routes;

/// <summary>
/// Route implementation for pages without typed parameters.
/// </summary>
internal class Route : RouteBase, IRoute
{
    /// <summary>
    /// The location path handler for this route.
    /// </summary>
    private readonly ILocationPath _path;

    /// <summary>
    /// Initializes a new instance of the Route class.
    /// </summary>
    /// <param name="navigationManager">The navigation manager for navigation operations.</param>
    /// <param name="template">The route template pattern.</param>
    /// <param name="pageType">The page component type associated with this route.</param>
    /// <param name="mapper">The mapper for data transformations.</param>
    public Route(NavigationManager navigationManager, string template, Type pageType, IMapper mapper)
        : base(navigationManager, template, pageType)
    {
        _path = LocationPath.Parse(template, [], mapper).Item1;
    }

    /// <summary>
    /// Generates a link URL for this route.
    /// </summary>
    /// <returns>The generated link URL.</returns>
    public string Link()
    {
        var path = _path.Link(new Dictionary<string, object?>());

        return path;
    }

    /// <summary>
    /// Navigates to this route.
    /// </summary>
    public void Go()
    {
        var link = Link();
        NavigationManager.NavigateTo(link);
    }

    /// <summary>
    /// Determines whether the current location matches this route.
    /// </summary>
    /// <param name="match">The path matching strategy to use.</param>
    /// <returns>True if the current location matches this route; otherwise, false.</returns>
    public bool IsAt(PathMatch match = PathMatch.Exact)
    {
        var raw = RawLocation.Parse(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));

        return Match(raw, match).IsSuccess;
    }

    /// <summary>
    /// Attempts to match a raw location against this route.
    /// </summary>
    /// <param name="raw">The raw location to match against.</param>
    /// <param name="match">The path matching strategy to use.</param>
    /// <returns>A location match result indicating success and any extracted route values.</returns>
    public override LocationMatch Match(RawLocation raw, PathMatch match)
    {
        var pathMatch = _path.Match(raw.Segments, match);

        return pathMatch;
    }
}
