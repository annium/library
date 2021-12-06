using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Css.Tests;

public class StylesheetTest
{
    [Fact]
    public void Stylesheet_Works()
    {
        // arrange
        var sp = new ServiceContainer()
            .AddRuntimeTools(GetType().Assembly, false)
            .AddCss()
            .BuildServiceProvider();
        var styleSheet = sp.Resolve<IStyleSheet>();

        // assert: before any RuleSet resolved - it's empty
        styleSheet.Css.Is(string.Empty);

        // act - resolve RuleSet
        sp.Resolve<Styles>();

        SpinWait.SpinUntil(() => styleSheet.Css.Length > 0, 100);

        // assert
        styleSheet.Css.IsNot(string.Empty);
    }
}

internal class Styles : RuleSet
{
    private CssRule _html = Rule.Tag("html")
        .Set("display", "flex")
        .Set("width", "100%")
        .Set("min-height", "100vh");
}