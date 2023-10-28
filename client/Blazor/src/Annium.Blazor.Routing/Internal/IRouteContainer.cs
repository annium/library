using Annium.Blazor.Routing.Internal.Routes;

namespace Annium.Blazor.Routing.Internal;

internal interface IRouteContainer
{
    void Track(IRouteBase route);
}
