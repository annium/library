using Annium.XRest.Core;
using Microsoft.Extensions.DependencyInjection;

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
