using Annium.Core.DependencyInjection;
using MatBlazor;

namespace Annium.Blazor.MatBlazor;

/// <summary>
/// Extension methods for configuring MatBlazor (Material Design) services in the dependency injection container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds MatBlazor services to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddMatBlazor(this IServiceContainer container)
    {
        container.Collection.AddMatBlazor();

        return container;
    }
}
