using Annium.Components.State;
using Annium.Components.State.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddComponentStateFactory(
            this IServiceCollection services
        )
        {
            services.AddSingleton<IStateFactory, StateFactory>();

            return services;
        }
    }
}