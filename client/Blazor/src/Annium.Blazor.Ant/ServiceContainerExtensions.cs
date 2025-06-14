using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring Ant Design services in the dependency injection container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds Ant Design services to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddAntDesign(this IServiceContainer container)
    {
        container.Collection.AddAntDesign();

        return container;
    }
}
