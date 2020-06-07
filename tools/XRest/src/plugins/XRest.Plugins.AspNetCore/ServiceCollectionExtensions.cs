using Microsoft.Extensions.DependencyInjection;

namespace XRest.Plugins.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddXRest(this IServiceCollection services)
        {
            services.AddOpenApiDocument();

            return services;
        }
    }
}