using Annium.Blazor.Interop;
using Microsoft.JSInterop;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddInterop(
        this IServiceContainer container
    )
    {
        container.Add(sp => (IJSInProcessRuntime)sp.Resolve<IJSRuntime>()).AsSelf().Singleton();
        container.Add(sp => (IJSUnmarshalledRuntime)sp.Resolve<IJSRuntime>()).AsSelf().Singleton();

        container.Add<IInteropContext, InteropContextInstance>().Singleton();
        container.OnBuild += InteropContext.Init;

        return container;
    }
}