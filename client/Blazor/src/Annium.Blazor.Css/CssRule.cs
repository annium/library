using System;

namespace Annium.Blazor.Css;

/// <summary>
/// Represents a top-level CSS rule that can contain media queries
/// </summary>
public abstract class CssTopLevelRule : CssRule
{
    /// <summary>
    /// Wraps the rule in a media query
    /// </summary>
    /// <param name="query">The media query string</param>
    /// <param name="configure">Action to configure the rule within the media query</param>
    /// <returns>The media query wrapped rule</returns>
    public abstract CssTopLevelRule Media(string query, Action<CssRule> configure);
}

/// <summary>
/// Base class for CSS rules that provides common functionality for styling elements
/// </summary>
public abstract class CssRule
{
    /// <summary>
    /// Gets or sets the name of the CSS rule
    /// </summary>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Sets a CSS property with the specified value
    /// </summary>
    /// <param name="property">The CSS property name</param>
    /// <param name="value">The CSS property value</param>
    /// <returns>The current rule instance for method chaining</returns>
    public abstract CssRule Set(string property, string value);

    /// <summary>
    /// Adds an additional selector to the rule using AND logic
    /// </summary>
    /// <param name="selector">The additional selector</param>
    /// <param name="configure">Action to configure the combined rule</param>
    /// <returns>The current rule instance for method chaining</returns>
    public abstract CssRule And(string selector, Action<CssRule> configure);

    /// <summary>
    /// Adds a child selector to the rule
    /// </summary>
    /// <param name="selector">The child selector</param>
    /// <param name="configure">Action to configure the child rule</param>
    /// <returns>The current rule instance for method chaining</returns>
    public abstract CssRule Child(string selector, Action<CssRule> configure);

    /// <summary>
    /// Adds an inheritor selector to the rule
    /// </summary>
    /// <param name="selector">The inheritor selector</param>
    /// <param name="configure">Action to configure the inheritor rule</param>
    /// <returns>The current rule instance for method chaining</returns>
    public abstract CssRule Inheritor(string selector, Action<CssRule> configure);

    /// <summary>
    /// Generates inline CSS styles for the rule
    /// </summary>
    /// <returns>The inline CSS string</returns>
    public abstract string Inline();

    /// <summary>
    /// Converts the rule to CSS format
    /// </summary>
    /// <returns>The CSS string representation</returns>
    public abstract string ToCss();

    /// <summary>
    /// Implicitly converts a CSS rule to its name string
    /// </summary>
    /// <param name="rule">The CSS rule to convert</param>
    /// <returns>The name of the CSS rule</returns>
    public static implicit operator string(CssRule rule) => rule.Name;
}
