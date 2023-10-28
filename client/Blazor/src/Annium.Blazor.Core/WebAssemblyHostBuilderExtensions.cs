using System;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Annium.Blazor.Core;

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

    public static async Task AddConfiguration<T>(
        this WebAssemblyHostBuilder builder,
        Func<IConfigurationContainer, Task> configure
    )
        where T : class, new()
    {
        var container = new ServiceContainer(builder.Services);
        await container.AddConfiguration<T>(configure);
    }
}
