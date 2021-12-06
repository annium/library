using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddXRest(this IServiceContainer container)
    {
        container.Collection.AddOpenApiDocument();

        return container;
    }
}