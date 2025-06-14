using Annium.Components.State.Forms;
using Annium.Components.State.Forms.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Provides extension methods for registering state form services in the service container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the state factory service as a singleton in the service container.
    /// </summary>
    /// <param name="container">The service container to register the service in.</param>
    /// <returns>The same service container instance for method chaining.</returns>
    public static IServiceContainer AddStateFactory(this IServiceContainer container)
    {
        container.Add<IStateFactory, StateFactory>().Singleton();

        return container;
    }
}
