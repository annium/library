using System.Reflection;
using Annium.Blazor.Net;
using Annium.Blazor.Net.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddHostHttpRequestFactory(this IServiceContainer container)
        {
            container.Add<IHostHttpRequestFactory, HostHttpRequestFactory>().Singleton();

            return container;
        }

        public static IServiceContainer AddApiServices(this IServiceContainer container)
        {
            container.AddAll(Assembly.GetCallingAssembly(), false)
                .AssignableTo<IApi>()
                .Where(x => x.IsClass)
                .AsInterfaces()
                .Singleton();
            container.AddAll(Assembly.GetCallingAssembly(), false)
                .AssignableTo<IApiService>()
                .Where(x => x.IsClass)
                .AsInterfaces()
                .Singleton();

            return container;
        }
    }
}