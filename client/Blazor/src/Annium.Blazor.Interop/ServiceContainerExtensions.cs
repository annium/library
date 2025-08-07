using Annium.Core.DependencyInjection;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop;

/// <summary>
/// Provides extension methods for configuring interop services in the service container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds interop services to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The configured service container.</returns>
    public static IServiceContainer AddInterop(this IServiceContainer container)
    {
        container.Add(sp => (IJSInProcessRuntime)sp.Resolve<IJSRuntime>()).AsSelf().Singleton();

        container.Add<IInteropContext, InteropContextInstance>().Singleton();
        container.OnBuild += InteropContext.Init;

        return container;
    }
}
