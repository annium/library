using Annium.Core.DependencyInjection;
using Annium.Net.Types.Serialization.Json;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.XRest.Core.Internal.Converters;

namespace Annium.XRest.Core;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddXRestSerializer(this IServiceContainer container)
    {
        container
            .AddSerializers(Constants.IndexKey)
            .WithJson(opts =>
            {
                opts.Converters.Add(new HttpMethodJsonConverter());
                opts.ConfigureForNetTypes();
                opts.UseCamelCaseNamingPolicy();
            });

        return container;
    }
}
