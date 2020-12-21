using System.Reflection;
using Annium.Blazor.Css;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddCss(this IServiceContainer container)
        {
            // register rule sets
            container.AddAll(Assembly.GetCallingAssembly())
                .AssignableTo<RuleSet>()
                .AsInterfaces()
                .AsSelf()
                .Singleton();

            // register stylesheet
            container.Add(Blazor.Css.Internal.StyleSheet.Instance).AsSelf().AsInterfaces().Singleton();

            return container;
        }
    }
}