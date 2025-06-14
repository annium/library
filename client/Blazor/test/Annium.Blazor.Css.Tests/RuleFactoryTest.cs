using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Css.Tests;

/// <summary>
/// Tests for the Rule factory functionality
/// </summary>
public class RuleFactoryTest
{
    /// <summary>
    /// Tests that the Rule.Class() method generates a valid class name automatically
    /// </summary>
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
