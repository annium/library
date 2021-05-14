using Annium.Blazor.Routing.Internal.Implementations.Routes;
using Annium.Core.Mapper;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Routing.Internal.Implementations
{
    internal class RouteFactory : IRouteFactory
    {
        private readonly NavigationManager _navigationManager;
        private readonly IRouteContainer _routeContainer;
        private readonly IMapper _mapper;

        public RouteFactory(
            NavigationManager navigationManager,
            IRouteContainer routeContainer,
            IMapper mapper
        )
        {
            _navigationManager = navigationManager;
            _routeContainer = routeContainer;
            _mapper = mapper;
        }

        public IRoute<TData> Create<TPage, TData>(string template, TData data = default)
            where TPage : notnull
            where TData : notnull, new()
        {
            var route = new Route<TData>(_navigationManager, template, typeof(TPage), _mapper);
            _routeContainer.Track(route);

            return route;
        }

        public IRoute Create<TPage>(string template)
            where TPage : notnull
        {
            var route = new Route(_navigationManager, template, typeof(TPage), _mapper);
            _routeContainer.Track(route);

            return route;
        }
    }
}