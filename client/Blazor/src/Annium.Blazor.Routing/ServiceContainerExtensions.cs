using System.Runtime.CompilerServices;
using Annium.Blazor.Routing;
using Annium.Blazor.Routing.Internal;
using Annium.Core.Runtime;

[assembly: InternalsVisibleTo("Annium.Blazor.Routing.Tests")]

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring Blazor routing services in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers Blazor routing services in the dependency injection container
    /// </summary>
    /// <param name="container">The service container to register services in</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddRouting(this IServiceContainer container)
    {
        container.Add<RouteManager>().AsInterfaces().Singleton();
        container.Add<RouteFactory>().AsInterfaces().Singleton();
        container.AddAll().AssignableTo<IRouting>().AsInterfaces().Singleton();

        return container;
    }
}
