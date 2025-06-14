using System;
using Annium.Blazor.Routing.Internal.Locations;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Routing.Internal.Routes;

/// <summary>
/// Abstract base class for route implementations providing common route functionality.
/// </summary>
internal abstract class RouteBase : IRouteBase
{
    /// <summary>
    /// Gets the route template pattern used for matching.
    /// </summary>
    public string Template { get; }

    /// <summary>
    /// Gets the page component type associated with this route.
    /// </summary>
    public Type PageType { get; }

    /// <summary>
    /// Gets the navigation manager used for navigation operations.
    /// </summary>
    protected NavigationManager NavigationManager { get; }

    /// <summary>
    /// Initializes a new instance of the RouteBase class.
    /// </summary>
    /// <param name="navigationManager">The navigation manager for navigation operations.</param>
    /// <param name="template">The route template pattern.</param>
    /// <param name="pageType">The page component type associated with this route.</param>
    protected RouteBase(NavigationManager navigationManager, string template, Type pageType)
    {
        NavigationManager = navigationManager;
        Template = template;
        PageType = pageType;
    }

    /// <summary>
    /// Attempts to match a raw location against this route.
    /// </summary>
    /// <param name="raw">The raw location to match against.</param>
    /// <param name="match">The path matching strategy to use.</param>
    /// <returns>A location match result indicating success and any extracted route values.</returns>
    public abstract LocationMatch Match(RawLocation raw, PathMatch match);
}
