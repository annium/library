using System.Reflection;
using Annium.Blazor.Net.Internal;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Blazor.Net
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHostHttpRequestFactory(this IServiceCollection services)
        {
            services.AddSingleton<IHostHttpRequestFactory, HostHttpRequestFactory>();

            return services;
        }

        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddAllTypes(Assembly.GetCallingAssembly())
                .AssignableTo<IApi>()
                .Where(x => x.IsClass)
                .AsImplementedInterfaces()
                .SingleInstance();
            services.AddAllTypes(Assembly.GetCallingAssembly())
                .AssignableTo<IApiService>()
                .Where(x => x.IsClass)
                .AsImplementedInterfaces()
                .SingleInstance();

            return services;
        }
    }
}