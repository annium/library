using Annium.Blazor.Routing.Internal.Routes;
using Annium.Core.Mapper;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Routing.Internal;

/// <summary>
/// Factory for creating and registering routes with the routing system.
/// </summary>
internal class RouteFactory : IRouteFactory
{
    /// <summary>
    /// The navigation manager for handling URI operations.
    /// </summary>
    private readonly NavigationManager _navigationManager;

    /// <summary>
    /// The route container for tracking created routes.
    /// </summary>
    private readonly IRouteContainer _routeContainer;

    /// <summary>
    /// The mapper for converting between different data types.
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the RouteFactory class.
    /// </summary>
    /// <param name="navigationManager">The navigation manager for handling URI operations.</param>
    /// <param name="routeContainer">The route container for tracking created routes.</param>
    /// <param name="mapper">The mapper for converting between different data types.</param>
    public RouteFactory(NavigationManager navigationManager, IRouteContainer routeContainer, IMapper mapper)
    {
        _navigationManager = navigationManager;
        _routeContainer = routeContainer;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a typed route with data binding for the specified page and data type.
    /// </summary>
    /// <typeparam name="TPage">The type of the page component.</typeparam>
    /// <typeparam name="TData">The type of the route data.</typeparam>
    /// <param name="template">The route template string.</param>
    /// <returns>The created route with data binding.</returns>
    public IRoute<TData> Create<TPage, TData>(string template)
        where TPage : notnull
        where TData : notnull, new()
    {
        var route = new Route<TData>(_navigationManager, template, typeof(TPage), _mapper);
        _routeContainer.Track(route);

        return route;
    }

    /// <summary>
    /// Creates a route for the specified page without data binding.
    /// </summary>
    /// <typeparam name="TPage">The type of the page component.</typeparam>
    /// <param name="template">The route template string.</param>
    /// <returns>The created route.</returns>
    public IRoute Create<TPage>(string template)
        where TPage : notnull
    {
        var route = new Route(_navigationManager, template, typeof(TPage), _mapper);
        _routeContainer.Track(route);

        return route;
    }
}
