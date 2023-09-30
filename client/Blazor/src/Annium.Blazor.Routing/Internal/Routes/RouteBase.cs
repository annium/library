using System;
using Annium.Blazor.Routing.Internal.Locations;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Routing.Internal.Routes;

internal abstract class RouteBase : IRouteBase
{
    public string Template { get; }
    public Type PageType { get; }
    protected NavigationManager NavigationManager { get; }

    protected RouteBase(
        NavigationManager navigationManager,
        string template,
        Type pageType
    )
    {
        NavigationManager = navigationManager;
        Template = template;
        PageType = pageType;
    }

    public abstract LocationMatch Match(RawLocation raw, PathMatch match);
}