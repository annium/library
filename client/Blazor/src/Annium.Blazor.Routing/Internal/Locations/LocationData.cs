using System;
using System.Collections.Generic;

namespace Annium.Blazor.Routing.Internal.Locations;

internal sealed record LocationData(Type PageType, IReadOnlyDictionary<string, object?> RouteValues);
