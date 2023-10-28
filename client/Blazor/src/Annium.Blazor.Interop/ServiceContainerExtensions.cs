using Annium.Blazor.Interop;
using Microsoft.JSInterop;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddInterop(this IServiceContainer container)
    {
        container.Add(sp => (IJSInProcessRuntime)sp.Resolve<IJSRuntime>()).AsSelf().Singleton();

        container.Add<IInteropContext, InteropContextInstance>().Singleton();
        container.OnBuild += InteropContext.Init;

        return container;
    }
}
