using Annium.Components.State.Forms;
using Annium.Components.State.Forms.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddComponentFormStateFactory(
            this IServiceCollection services
        )
        {
            services.AddSingleton<IStateFactory, StateFactory>();

            return services;
        }
    }
}