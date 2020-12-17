using System;
using System.Collections.Generic;
using System.Reflection;
using Annium.Blazor.Routing.Internal.Implementations.Locations;
using Annium.Blazor.Routing.Internal.Locations;
using Annium.Blazor.Routing.Internal.Routes;
using Annium.Core.Mapper;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Routing.Internal.Implementations.Routes
{
    internal class Route : RouteBase, IRoute
    {
        private readonly ILocationPath _path;

        public Route(
            NavigationManager navigationManager,
            string template,
            Type pageType,
            IMapper mapper
        ) : base(navigationManager, template, pageType)
        {
            _path = LocationPath.Parse(template, Array.Empty<PropertyInfo>(), mapper).Item1;
        }

        public string Link()
        {
            var path = _path.Link(new Dictionary<string, object>());

            return path;
        }

        public void Go()
        {
            var link = Link();
            NavigationManager.NavigateTo(link);
        }

        public bool IsAt(PathMatch match = PathMatch.Exact)
        {
            var raw = RawLocation.Parse(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));

            return Match(raw, match).IsSuccess;
        }

        public override LocationMatch Match(RawLocation raw, PathMatch match)
        {
            var pathMatch = _path.Match(raw.Segments, match);

            return pathMatch;
        }
    }
}