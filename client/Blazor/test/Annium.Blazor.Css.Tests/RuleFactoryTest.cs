using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Css.Tests;

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
}
