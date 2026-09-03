using Annium.Blazor.Routing.Internal.Routes;

namespace Annium.Blazor.Routing.Internal;

/// <summary>
/// Provides functionality for tracking and managing routes within the routing system.
/// </summary>
internal interface IRouteContainer
{
    /// <summary>
    /// Registers a route for tracking within the container.
    /// </summary>
    /// <param name="route">The route to track</param>
    void Track(IRouteBase route);
}
