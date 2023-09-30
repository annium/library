using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Routing.Internal.Locations;
using Annium.Blazor.Routing.Internal.Routes;

namespace Annium.Blazor.Routing.Internal;

internal class RouteManager : IRouteMatcher, IRouteContainer
{
    private readonly IList<IRouteBase> _routes = new List<IRouteBase>();

    public void Track(IRouteBase route)
    {
        _routes.Add(route);
    }

    public LocationData? Match(RawLocation rawLocation, PathMatch match) => _routes
        .Select(x => (type: x.PageType, match: x.Match(rawLocation, match)))
        .Where(x => x.match.IsSuccess)
        .Select(x => new LocationData(x.type, x.match.RouteValues))
        .FirstOrDefault();
}