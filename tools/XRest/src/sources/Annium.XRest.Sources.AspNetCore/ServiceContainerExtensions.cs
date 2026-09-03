using Annium.Core.DependencyInjection;
using Annium.Net.Types;
using Annium.XRest.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.XRest.Sources.AspNetCore;

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
