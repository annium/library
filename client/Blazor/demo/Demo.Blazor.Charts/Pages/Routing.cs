using Annium.Blazor.Routing;

namespace Demo.Blazor.Charts.Pages;

public class Routing : IRouting
{
    public IRoute Home { get; }

    public Routing(
        IRouteFactory routeFactory
    )
    {
        Home = routeFactory.Create<Home.Page>("/");
    }
}