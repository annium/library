namespace Annium.Blazor.Css;

/// <summary>
/// Base class for CSS rule sets that automatically register themselves with the stylesheet
/// </summary>
public abstract class RuleSet
{
    /// <summary>
    /// Initializes a new instance of the RuleSet class and registers it with the stylesheet
    /// </summary>
    protected RuleSet()
    {
        Internal.StyleSheet.Instance.Add(this);
    }
}
