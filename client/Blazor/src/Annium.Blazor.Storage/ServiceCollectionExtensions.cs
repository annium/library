using Annium.Blazor.Storage;
using Annium.Blazor.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBrowserStorage(this IServiceCollection services)
        {
            services.AddSingleton<ILocalStorage, LocalStorage>();
            services.AddSingleton<ISessionStorage, SessionStorage>();

            return services;
        }
    }
}