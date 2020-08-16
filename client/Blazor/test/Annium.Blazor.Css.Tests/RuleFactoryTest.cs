using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Css.Tests
{
    public class RuleFactoryTest
    {
        [Fact]
        public void Rule_Class_Auto_Ok()
        {
            // arrange
            var rule = Rule.Class();

            // act
            var name = rule.ToString();

            // assert
            name.IsNotDefault();
        }

        [Fact]
        public void Rule_Media_Nesting_Ok()
        {
            // arrange
            var rule = Rule.Media("screen")
                .WidthPx(10)
                .Inheritor(".active", active => active
                    .HeightPx(5)
                    .Child("a", link => link.FontSizePx(12))
                );

            // act
            var css = rule.ToCss();

            // assert
            css.IsNotDefault();
        }
    }
}