using System.Reflection;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Blazor.Css
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCssRules(this IServiceCollection services)
        {
            // register rule sets
            services.AddAssemblyTypes(Assembly.GetCallingAssembly())
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