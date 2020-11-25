using System.Reflection;
using Annium.Core.DependencyInjection;

namespace Annium.Blazor.Css
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddCssRules(this IServiceContainer container)
        {
            // register rule sets
            container.AddAll(Assembly.GetCallingAssembly())
                .AssignableTo<IRuleSet>()
                .AsInterfaces()
                .AsSelf()
                .Singleton();

            // register stylesheet
            container.Add<IStyleSheet, Internal.StyleSheet>().Singleton();

            return container;
        }
    }
}