using Annium.Blazor.Routing;
using Demo.Blazor.Interop.Pages.Canvas;

namespace Demo.Blazor.Interop.Pages;

/// <summary>
/// Defines the routing configuration for the Blazor Interop demo application.
/// </summary>
public class Routing : IRouting
{
    /// <summary>
    /// Gets the route for the Canvas demo page.
    /// </summary>
    public IRoute Canvas { get; }

    /// <summary>
    /// Gets the route for the Element demo page.
    /// </summary>
    public IRoute Element { get; }

    /// <summary>
    /// Initializes a new instance of the Routing class with the specified route factory.
    /// </summary>
    /// <param name="routeFactory">The factory used to create routes.</param>
    public Routing(IRouteFactory routeFactory)
    {
        Canvas = routeFactory.Create<Page>("/canvas");
        Element = routeFactory.Create<Element.Page>("/element");
    }
}
