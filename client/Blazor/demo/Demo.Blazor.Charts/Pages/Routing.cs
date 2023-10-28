using Annium.Blazor.Routing;
using Demo.Blazor.Charts.Pages.Home;

namespace Demo.Blazor.Charts.Pages;

public class Routing : IRouting
{
    public IRoute Home { get; }

    public Routing(IRouteFactory routeFactory)
    {
        Home = routeFactory.Create<Page>("/");
    }
}
