using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Annium.Blazor.Core.Extensions;

public static class WebAssemblyHostBuilderExtensions
{
    public static WebAssemblyHostBuilder StartAt<TApp>(this WebAssemblyHostBuilder builder)
        where TApp : IComponent
    {
        builder.RootComponents.Add<TApp>(typeof(TApp).FriendlyName());

        return builder;
    }

    public static WebAssemblyHostBuilder UseServicePack<T>(this WebAssemblyHostBuilder builder)
        where T : ServicePackBase, new()
    {
        builder.ConfigureContainer(new ServiceProviderFactory(b => b.UseServicePack<T>()));

        return builder;
    }
}