using System.Collections.Generic;
using System.Linq;
using Annium.AI.SemanticKernel.Internal;
using Annium.Core.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Annium.AI.SemanticKernel;

/// <summary>
/// Container extensions that register the Semantic Kernel itself.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers a transient <see cref="Kernel"/> whose plugins are the union of every registered
    /// <see cref="KernelPluginCollection"/>, and returns a builder for adding AI services and plugin sources.
    /// </summary>
    /// <remarks>
    /// Plugin names must be unique across all sources: the union is not de-duplicated, and
    /// <see cref="KernelPluginCollection"/> rejects a repeated name, so a collision makes every kernel
    /// resolution throw <see cref="System.ArgumentException"/> rather than silently dropping one plugin.
    /// </remarks>
    /// <param name="container">The container to register into.</param>
    /// <returns>A builder for further Semantic Kernel registrations.</returns>
    public static ISemanticKernelBuilder AddSemanticKernel(this IServiceContainer container)
    {
        container
            .Add(static sp =>
            {
                var plugins = sp.Resolve<IEnumerable<KernelPluginCollection>>().SelectMany(x => x).ToArray();
                var pluginsCollection = new KernelPluginCollection(plugins);

                return new Kernel(sp, pluginsCollection);
            })
            .AsSelf()
            .Transient();

        return new SemanticKernelBuilder(container);
    }
}
