using Annium.Blazor.Routing;
using Demo.Blazor.Ant.Pages.Home;

namespace Demo.Blazor.Ant.Pages;

/// <summary>
/// Routing configuration for the Demo.Blazor.Ant application.
/// </summary>
public class Routing : IRouting
{
    /// <summary>
    /// Gets the route for the home page.
    /// </summary>
    public IRoute Home { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Routing"/> class.
    /// </summary>
    /// <param name="routeFactory">The route factory for creating routes.</param>
    public Routing(IRouteFactory routeFactory)
    {
        Home = routeFactory.Create<Page>("/");
    }
}
