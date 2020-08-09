using Annium.Components.Forms;
using Annium.Components.Forms.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFormsState(
            this IServiceCollection services
        )
        {
            services.AddSingleton<IStateFactory, StateFactory>();

            return services;
        }
    }
}