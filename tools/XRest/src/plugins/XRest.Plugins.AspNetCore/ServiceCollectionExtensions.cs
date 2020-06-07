using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
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