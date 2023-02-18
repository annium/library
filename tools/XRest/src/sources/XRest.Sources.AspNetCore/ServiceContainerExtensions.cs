using Microsoft.Extensions.DependencyInjection;
using XRest.Core;

// ReSharper disable once CheckNamespace

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddXRest(this IServiceContainer container)
    {
        container.Collection.AddOpenApiDocument();
        container.AddXRestSerializer();
        container.AddModelMapper();

        return container;
    }
}