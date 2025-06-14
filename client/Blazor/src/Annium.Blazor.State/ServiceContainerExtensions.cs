using Annium.Blazor.State;
using Annium.Blazor.State.Internal;
using Annium.Core.Runtime;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring Blazor state management services in the dependency injection container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds state management services including storage providers and state classes to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddStates(this IServiceContainer container)
    {
        container.Add<ILocalStorage, LocalStorage>().Singleton();
        container.Add<ISessionStorage, SessionStorage>().Singleton();

        container
            .AddAll()
            .AssignableTo<StateBase>()
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .AsSelf()
            .AsInterfaces()
            .Scoped();

        return container;
    }
}
