using System.Collections.Generic;
using Annium.Blazor.Routing.Internal.Locations;
using Annium.Blazor.Routing.Internal.Routes;
using Annium.Logging;

namespace Annium.Blazor.Routing.Internal;

/// <summary>
/// Manages route registration and matching operations for the routing system.
/// </summary>
internal class RouteManager : IRouteMatcher, IRouteContainer, ILogSubject
{
    /// <summary>
    /// Gets the logger for this route manager.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The collection of registered routes.
    /// </summary>
    private readonly IList<IRouteBase> _routes = new List<IRouteBase>();

    /// <summary>
    /// Initializes a new instance of the RouteManager class.
    /// </summary>
    /// <param name="logger">The logger to use for this route manager.</param>
    public RouteManager(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Tracks a route by adding it to the collection of registered routes.
    /// </summary>
    /// <param name="route">The route to track.</param>
    public void Track(IRouteBase route)
    {
        _routes.Add(route);
    }

    /// <summary>
    /// Attempts to match a raw location against registered routes.
    /// </summary>
    /// <param name="rawLocation">The raw location to match.</param>
    /// <param name="pathMatch">The path matching strategy to use.</param>
    /// <returns>Location data if a match is found; otherwise, null.</returns>
    public LocationData? Match(RawLocation rawLocation, PathMatch pathMatch)
    {
        this.Trace("start, check {count} routes", _routes.Count);

        foreach (var route in _routes)
        {
            var match = route.Match(rawLocation, pathMatch);

            this.Trace("{route} - {result}", route.Template, match.IsSuccess);

            if (match.IsSuccess)
                return new LocationData(route.PageType, match.RouteValues);
        }

        this.Trace("done, no route matched");

        return null;
    }
}
