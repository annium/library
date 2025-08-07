using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;

namespace Annium.Blazor.Css;

/// <summary>
/// Extension methods for configuring CSS services in the dependency injection container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds CSS services including rule sets and stylesheet to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The configured service container for method chaining.</returns>
    public static IServiceContainer AddCss(this IServiceContainer container)
    {
        // register rule sets
        container.AddAll().AssignableTo<RuleSet>().AsSelf().Singleton();

        // register stylesheet
        container.Add(Internal.StyleSheet.Instance).AsSelf().AsInterfaces().Singleton();

        return container;
    }
}
