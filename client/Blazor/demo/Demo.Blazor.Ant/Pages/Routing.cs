using Annium.Blazor.Routing;
using Demo.Blazor.Ant.Pages.Home;

namespace Demo.Blazor.Ant.Pages;

public class Routing : IRouting
{
    public IRoute Home { get; }

    public Routing(IRouteFactory routeFactory)
    {
        Home = routeFactory.Create<Page>("/");
    }
}
