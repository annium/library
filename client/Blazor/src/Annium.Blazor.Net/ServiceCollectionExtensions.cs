using Annium.Blazor.Net.Internal;
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
    }
}