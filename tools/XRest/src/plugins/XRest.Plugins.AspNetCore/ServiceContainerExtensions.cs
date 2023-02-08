using Microsoft.Extensions.DependencyInjection;
using XRest.Plugins.AspNetCore;

// ReSharper disable once CheckNamespace

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddXRest(this IServiceContainer container)
    {
        container.Collection.AddOpenApiDocument();
        container.Add<ApiModelBuilder>().AsSelf().Singleton();

        return container;
    }
}