using System;
using Annium.Blazor.Routing.Internal.Locations;

namespace Annium.Blazor.Routing.Internal.Routes;

internal interface IRouteBase
{
    string Template { get; }
    Type PageType { get; }
    LocationMatch Match(RawLocation raw, PathMatch match);
}
