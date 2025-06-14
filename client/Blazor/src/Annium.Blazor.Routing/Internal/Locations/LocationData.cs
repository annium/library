using System;
using System.Collections.Generic;

namespace Annium.Blazor.Routing.Internal.Locations;

/// <summary>
/// Represents the data associated with a matched location, including the target page type and extracted route values.
/// </summary>
/// <param name="PageType">The type of the page component that should be rendered for this location</param>
/// <param name="RouteValues">The route parameter values extracted from the matched location</param>
internal sealed record LocationData(Type PageType, IReadOnlyDictionary<string, object?> RouteValues);
