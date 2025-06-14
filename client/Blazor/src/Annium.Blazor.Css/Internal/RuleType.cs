namespace Annium.Blazor.Css.Internal;

/// <summary>
/// Represents CSS rule type prefixes such as class (.) and ID (#) selectors.
/// </summary>
internal class RuleType : ImplicitString<RuleType>
{
    /// <summary>
    /// CSS ID selector prefix (#).
    /// </summary>
    public static readonly RuleType Id = new("#");

    /// <summary>
    /// CSS class selector prefix (.).
    /// </summary>
    public static readonly RuleType Class = new(".");

    /// <summary>
    /// Initializes a new instance of the RuleType class with the specified type prefix.
    /// </summary>
    /// <param name="type">The CSS selector prefix.</param>
    private RuleType(string type)
        : base(type) { }
}
