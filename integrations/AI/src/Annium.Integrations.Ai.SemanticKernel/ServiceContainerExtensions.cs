using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Integrations.Ai.SemanticKernel.Internal;
using Microsoft.SemanticKernel;

namespace Annium.Integrations.Ai.SemanticKernel;

public static class ServiceContainerExtensions
{
    public static ISemanticKernelBuilder AddKernel(this IServiceContainer container)
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
