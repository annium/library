using Annium.Blazor.Net;
using Annium.Blazor.Net.Internal;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddHostHttpRequestFactory(this IServiceContainer container)
    {
        container.Add<IHostHttpRequestFactory, HostHttpRequestFactory>().Singleton();

        return container;
    }

    public static IServiceContainer AddApiServices(this IServiceContainer container)
    {
        container.AddAll()
            .AssignableTo<IApi>()
            .Where(x => x.IsClass)
            .AsInterfaces()
            .Scoped();
        container.AddAll()
            .AssignableTo<IApiService>()
            .Where(x => x.IsClass)
            .AsInterfaces()
            .Scoped();

        return container;
    }
}