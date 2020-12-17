using Annium.Blazor.Routing.Internal.Implementations.Routes;
using Annium.Blazor.Routing.Internal.Locations;

namespace Annium.Blazor.Routing.Internal.Routes
{
    public interface IRoute
    {
        string Link();
        void Go();
        bool IsAt(PathMatch match = PathMatch.Exact);
    }
}
