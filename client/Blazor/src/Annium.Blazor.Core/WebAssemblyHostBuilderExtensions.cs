using System;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Annium.Blazor.Core;

/// <summary>
/// Extension methods for configuring WebAssembly host builders.
/// </summary>
public static class WebAssemblyHostBuilderExtensions
{
    /// <summary>
    /// Configures the WebAssembly host to start with the specified root component.
    /// </summary>
    /// <typeparam name="TApp">The type of the root component.</typeparam>
    /// <param name="builder">The WebAssembly host builder.</param>
    /// <returns>The WebAssembly host builder for method chaining.</returns>
    public static WebAssemblyHostBuilder StartAt<TApp>(this WebAssemblyHostBuilder builder)
        where TApp : IComponent
    {
        builder.RootComponents.Add<TApp>(typeof(TApp).FriendlyName());

        return builder;
    }

    /// <summary>
    /// Configures the WebAssembly host to use the specified service pack for dependency injection.
    /// </summary>
    /// <typeparam name="T">The type of the service pack.</typeparam>
    /// <param name="builder">The WebAssembly host builder.</param>
    /// <returns>The WebAssembly host builder for method chaining.</returns>
    public static WebAssemblyHostBuilder UseServicePack<T>(this WebAssemblyHostBuilder builder)
        where T : ServicePackBase, new()
    {
        builder.ConfigureContainer(new ServiceProviderFactory(b => b.UseServicePack<T>()));

        return builder;
    }

    /// <summary>
    /// Asynchronously adds configuration to the WebAssembly host builder.
    /// </summary>
    /// <typeparam name="T">The type of the configuration object.</typeparam>
    /// <param name="builder">The WebAssembly host builder.</param>
    /// <param name="configure">The function to configure the configuration container.</param>
    /// <returns>A task representing the async configuration operation.</returns>
    public static async Task AddConfigurationAsync<T>(
        this WebAssemblyHostBuilder builder,
        Func<IConfigurationContainer, Task> configure
    )
        where T : class, new()
    {
        var container = new ServiceContainer(builder.Services);
        await container.AddConfigurationAsync<T>(configure);
    }
}
