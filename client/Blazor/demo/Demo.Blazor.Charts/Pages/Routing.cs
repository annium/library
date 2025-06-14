using Annium.Blazor.Routing;
using Demo.Blazor.Charts.Pages.Home;

namespace Demo.Blazor.Charts.Pages;

/// <summary>
/// Routing configuration for the Charts demo application
/// </summary>
public class Routing : IRouting
{
    /// <summary>
    /// Gets the route to the Home page
    /// </summary>
    public IRoute Home { get; }

    /// <summary>
    /// Initializes a new instance of the Routing class
    /// </summary>
    /// <param name="routeFactory">Factory for creating routes</param>
    public Routing(IRouteFactory routeFactory)
    {
        Home = routeFactory.Create<Page>("/");
    }
}
