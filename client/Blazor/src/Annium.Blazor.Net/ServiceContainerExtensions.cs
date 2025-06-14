using Annium.Blazor.Net;
using Annium.Blazor.Net.Internal;
using Annium.Core.Runtime;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring Blazor.Net services in the service container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds the host HTTP request factory to the service container as a singleton.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The service container for chaining.</returns>
    public static IServiceContainer AddHostHttpRequestFactory(this IServiceContainer container)
    {
        container.Add<IHostHttpRequestFactory, HostHttpRequestFactory>().Singleton();

        return container;
    }

    /// <summary>
    /// Adds all API and API service implementations to the service container as scoped services.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The service container for chaining.</returns>
    public static IServiceContainer AddApiServices(this IServiceContainer container)
    {
        container.AddAll().AssignableTo<IApi>().Where(x => x.IsClass).AsInterfaces().Scoped();
        container.AddAll().AssignableTo<IApiService>().Where(x => x.IsClass).AsInterfaces().Scoped();

        return container;
    }
}
