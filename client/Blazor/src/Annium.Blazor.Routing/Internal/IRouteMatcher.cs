using Annium.Blazor.Routing.Internal.Locations;

namespace Annium.Blazor.Routing.Internal
{
    internal interface IRouteMatcher
    {
        LocationData? Match(RawLocation rawLocation, PathMatch match);
    }
}