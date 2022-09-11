using Annium.Blazor.Routing;
using Demo.Blazor.Interop.Pages.Canvas;

namespace Demo.Blazor.Interop.Pages;

public class Routing : IRouting
{
    public IRoute Canvas { get; }
    public IRoute Element { get; }

    public Routing(
        IRouteFactory routeFactory
    )
    {
        Canvas = routeFactory.Create<Page>("/canvas");
        Element = routeFactory.Create<Element.Page>("/element");
    }
}