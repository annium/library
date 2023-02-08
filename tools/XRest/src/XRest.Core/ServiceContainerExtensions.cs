using Annium.Core.DependencyInjection;
using XRest.Core.Internal.Converters;

namespace XRest.Core;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddXRestSerializer(this IServiceContainer container)
    {
        container.AddSerializers(Constants.IndexKey)
            .WithJson(opts =>
            {
                opts.Converters.Add(new HttpMethodJsonConverter());
                opts.Converters.Add(new NamespaceJsonConverter());
                opts.UseCamelCaseNamingPolicy();
            });

        return container;
    }
}