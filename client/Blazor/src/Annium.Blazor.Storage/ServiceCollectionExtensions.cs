using System.Reflection;
using Annium.Blazor.Storage;
using Annium.Blazor.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStorages(this IServiceCollection services)
        {
            services.AddSingleton<ILocalStorage, LocalStorage>();
            services.AddSingleton<ISessionStorage, SessionStorage>();

            services.AddAllTypes(Assembly.GetCallingAssembly(), false)
                .AssignableTo<IStore>()
                .Where(x => x.IsClass)
                .AsImplementedInterfaces()
                .SingleInstance();

            return services;
        }
    }
}