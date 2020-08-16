using Annium.Core.DependencyInjection;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Blazor.Css.Tests
{
    public class StylesheetTest
    {
        [Fact]
        public void Stylesheet_Works()
        {
            // arrange
            var styleSheet = new ServiceCollection()
                .AddRuntimeTools(GetType().Assembly)
                .AddCssRules()
                .BuildServiceProvider()
                .GetRequiredService<IStyleSheet>();

            // act
            var css = styleSheet.ToCss();

            // assert
            css.IsNotDefault();
        }
    }

    internal class Styles : IRuleSet
    {
        private IRule _html = Rule.Tag("html")
            .Set("display", "flex")
            .Set("width", "100%")
            .Set("min-height", "100vh");
    }
}