using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Css.Tests;

public class StylesheetTest
{
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

internal class Styles : RuleSet
{
    public readonly CssRule Html = Rule.Tag("html")
        .Set("display", "flex")
        .Set("width", "100%")
        .Set("min-height", "100vh");
}
