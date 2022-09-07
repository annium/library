using Annium.Blazor.Routing;

namespace Demo.Blazor.Interop.Pages;

public class Routing : IRouting
{
    public IRoute Canvas { get; }
    public IRoute Element { get; }

    public Routing(
        IRouteFactory routeFactory
    )
    {
        Canvas = routeFactory.Create<Canvas.Page>("/canvas");
        Element = routeFactory.Create<Element.Page>("/element");
    }
}