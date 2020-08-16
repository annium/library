using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Blazor.Css
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCssRules(this IServiceCollection services)
        {
            // register rule sets
            services.AddAllTypes()
                .AssignableTo<IRuleSet>()
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            // register stylesheet
            services.AddSingleton<IStyleSheet, Internal.StyleSheet>();

            return services;
        }
    }
}