using System.Collections.Generic;
using Annium.Blazor.Routing.Internal.Locations;
using Annium.Blazor.Routing.Internal.Routes;
using Annium.Logging;

namespace Annium.Blazor.Routing.Internal;

internal class RouteManager : IRouteMatcher, IRouteContainer, ILogSubject
{
    public ILogger Logger { get; }
    private readonly IList<IRouteBase> _routes = new List<IRouteBase>();

    public RouteManager(ILogger logger)
    {
        Logger = logger;
    }

    public void Track(IRouteBase route)
    {
        _routes.Add(route);
    }

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