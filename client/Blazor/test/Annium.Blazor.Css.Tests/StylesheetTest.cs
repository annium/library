using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Css.Tests;

/// <summary>
/// Contains tests for stylesheet functionality
/// </summary>
public class StylesheetTest
{
    /// <summary>
    /// Tests that stylesheet correctly generates CSS from resolved RuleSets
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Stylesheet_Works()
    {
        // arrange
        var sp = new ServiceContainer().AddRuntime(GetType().Assembly).AddCss().BuildServiceProvider();
        var styleSheet = sp.Resolve<IStyleSheet>();

        // assert: before any RuleSet resolved - it's empty
        styleSheet.Css.Is(string.Empty);

        // act - resolve RuleSet
        sp.Resolve<Styles>();

        await Expect.ToAsync(() => styleSheet.Css.IsNot(string.Empty), 100);
    }
}

/// <summary>
/// Test CSS rule set containing HTML styling rules
/// </summary>
internal class Styles : RuleSet
{
    /// <summary>
    /// CSS rule for HTML tag with flex layout and full viewport height
    /// </summary>
    public readonly CssRule Html = Rule.Tag("html")
        .Set("display", "flex")
        .Set("width", "100%")
        .Set("min-height", "100vh");
}
