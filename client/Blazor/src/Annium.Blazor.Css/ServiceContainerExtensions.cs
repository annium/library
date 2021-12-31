using Annium.Blazor.Css;
using StyleSheet = Annium.Blazor.Css.Internal.StyleSheet;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddCss(this IServiceContainer container)
    {
        // register rule sets
        container.AddAll()
            .AssignableTo<RuleSet>()
            .AsInterfaces()
            .AsSelf()
            .Singleton();

        // register stylesheet
        container.Add(StyleSheet.Instance).AsSelf().AsInterfaces().Singleton();

        return container;
    }
}