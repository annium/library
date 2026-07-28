using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Css.Tests;

/// <summary>
/// Tests for the Rule factory functionality
/// </summary>
public class RuleFactoryTest
{
    /// <summary>
    /// Tests that the parameterless Rule.Class() generates a class selector from caller info: it is prefixed with
    /// '.' and embeds the calling member name (pins the class prefix + generated-name shape, which a bare
    /// not-default check cannot).
    /// </summary>
    [Fact]
    public void Rule_Class_Auto_Ok()
    {
        // arrange
        var rule = Rule.Class();

        // act
        var name = rule.ToString();

        // assert: a class selector (leading '.') with a generated, non-default name
        name.IsNotDefault();
        name.StartsWith(".").IsTrue();
#if DEBUG
        // DEBUG embeds caller info; RELEASE uses a compact "a{index}" name with no member name
        name.Contains(nameof(Rule_Class_Auto_Ok)).IsTrue();
#else
        System.Text.RegularExpressions.Regex.IsMatch(name, @"^\.a\d+$").IsTrue();
#endif
    }
}
