using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Annium.Blazor.Css.Internal;

/// <summary>
/// Internal implementation of CSS rule that supports nested rules, media queries, and property management.
/// </summary>
internal class CssRuleInternal : CssTopLevelRule
{
    /// <summary>
    /// The CSS selector for this rule.
    /// </summary>
    private readonly string _selector;

    /// <summary>
    /// Collection of nested CSS rules.
    /// </summary>
    private readonly IList<CssRuleInternal> _rules = new List<CssRuleInternal>();

    /// <summary>
    /// Dictionary of media query rules keyed by media query string.
    /// </summary>
    private readonly IDictionary<string, CssRuleInternal> _media = new Dictionary<string, CssRuleInternal>();

    /// <summary>
    /// Dictionary of CSS properties and their values.
    /// </summary>
    private readonly IDictionary<string, string> _properties = new Dictionary<string, string>();

#if DEBUG
    /// <summary>
    /// Indentation size for formatted CSS output in debug mode.
    /// </summary>
    private const int Indent = 2;

    /// <summary>
    /// Converts a key-value pair to CSS property string with formatting.
    /// </summary>
    /// <param name="pair">The property key-value pair.</param>
    /// <returns>Formatted CSS property string.</returns>
    private static string PropertyToCss(KeyValuePair<string, string> pair) => $"{pair.Key}: {pair.Value};";

    /// <summary>
    /// Writes CSS rule to StringBuilder with proper indentation for debug mode.
    /// </summary>
    /// <param name="inheritedSelector">The inherited selector from parent rules.</param>
    /// <param name="rule">The CSS rule to write.</param>
    /// <param name="sb">The StringBuilder to write to.</param>
    /// <param name="indent">The current indentation level.</param>
    private static void WriteCss(string inheritedSelector, CssRuleInternal rule, StringBuilder sb, int indent = 0)
    {
        var i1 = new string(' ', indent);
        var i2 = new string(' ', indent + Indent);

        // this rule
        sb.AppendLine($"{i1}{inheritedSelector}{rule._selector} {{");
        foreach (var property in rule._properties.Select(PropertyToCss))
            sb.AppendLine($"{i2}{property}");
        sb.AppendLine($"{i1}}}");

        // inner rules
        foreach (var innerRule in rule._rules)
        {
            sb.AppendLine();
            WriteCss($"{inheritedSelector}{rule._selector}", innerRule, sb, indent);
        }

        // media rules
        foreach (var (query, mediaRule) in rule._media)
        {
            sb.AppendLine();
            sb.AppendLine($"{i1}{query} {{");
            WriteCss(inheritedSelector, mediaRule, sb, indent + Indent);
            sb.AppendLine($"{i1}}}");
        }
    }
#else
    /// <summary>
    /// Converts a key-value pair to minified CSS property string.
    /// </summary>
    /// <param name="pair">The property key-value pair.</param>
    /// <returns>Minified CSS property string.</returns>
    private static string PropertyToCss(KeyValuePair<string, string> pair) => $"{pair.Key}:{pair.Value};";

    /// <summary>
    /// Writes CSS rule to StringBuilder in minified format for release mode.
    /// </summary>
    /// <param name="inheritedSelector">The inherited selector from parent rules.</param>
    /// <param name="rule">The CSS rule to write.</param>
    /// <param name="sb">The StringBuilder to write to.</param>
    private static void WriteCss(string inheritedSelector, CssRuleInternal rule, StringBuilder sb)
    {
        // skip
        if (rule._properties.Count == 0 && rule._rules.Count == 0 && rule._media.Count == 0)
            return;

        // this rule
        sb.Append($"{inheritedSelector}{rule._selector}{{");
        foreach (var property in rule._properties.Select(PropertyToCss))
            sb.Append(property);
        sb.Append("}");

        // inner rules
        foreach (var innerRule in rule._rules)
            WriteCss($"{inheritedSelector}{rule._selector}", innerRule, sb);

        // media rules
        foreach (var (query, mediaRule) in rule._media)
        {
            sb.AppendLine($"{query} {{");
            WriteCss(inheritedSelector, mediaRule, sb);
            sb.AppendLine("}");
        }
    }
#endif

    /// <summary>
    /// Writes CSS rule properties as inline style string.
    /// </summary>
    /// <param name="rule">The CSS rule to write.</param>
    /// <param name="sb">The StringBuilder to write to.</param>
    private static void WriteInline(CssRuleInternal rule, StringBuilder sb)
    {
        // inner rules and media queries are forbidden
        if (rule._rules.Count != 0 || rule._media.Count != 0)
            throw new InvalidOperationException("Inner rules and media queries are not allowed in inline rules");

        foreach (var property in rule._properties.Select(PropertyToCss))
            sb.Append(property);
    }

    /// <summary>
    /// Initializes a new instance of the CssRuleInternal class with the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector for this rule.</param>
    public CssRuleInternal(string selector)
    {
        Name = selector.Split('#', '.').Last();
        _selector = selector;
    }

    /// <summary>
    /// Sets a CSS property value for this rule.
    /// </summary>
    /// <param name="property">The CSS property name.</param>
    /// <param name="value">The CSS property value.</param>
    /// <returns>The current CSS rule instance for method chaining.</returns>
    public override CssRule Set(string property, string value)
    {
        _properties[property] = value;

        return this;
    }

    /// <summary>
    /// Adds a nested rule with the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector for the nested rule.</param>
    /// <param name="configure">Action to configure the nested rule.</param>
    /// <returns>The current CSS rule instance for method chaining.</returns>
    public override CssRule And(string selector, Action<CssRule> configure) => AddRule(selector, configure);

#if DEBUG
    /// <summary>
    /// Adds a child rule with the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector for the child rule.</param>
    /// <param name="configure">Action to configure the child rule.</param>
    /// <returns>The current CSS rule instance for method chaining.</returns>
    public override CssRule Child(string selector, Action<CssRule> configure) => AddRule($" > {selector}", configure);
#else
    /// <summary>
    /// Adds a child rule with the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector for the child rule.</param>
    /// <param name="configure">Action to configure the child rule.</param>
    /// <returns>The current CSS rule instance for method chaining.</returns>
    public override CssRule Child(string selector, Action<CssRule> configure) => AddRule($">{selector}", configure);
#endif

    /// <summary>
    /// Adds an inheritor rule with the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector for the inheritor rule.</param>
    /// <param name="configure">Action to configure the inheritor rule.</param>
    /// <returns>The current CSS rule instance for method chaining.</returns>
    public override CssRule Inheritor(string selector, Action<CssRule> configure) => AddRule($" {selector}", configure);

    /// <summary>
    /// Adds a media query rule with the specified query.
    /// </summary>
    /// <param name="query">The media query string.</param>
    /// <param name="configure">Action to configure the media query rule.</param>
    /// <returns>The current CSS rule instance for method chaining.</returns>
    public override CssTopLevelRule Media(string query, Action<CssRule> configure)
    {
        var rule = new CssRuleInternal(_selector);
        configure(rule);
        _media[$"@media {query}"] = rule;

        return this;
    }

    /// <summary>
    /// Returns the CSS selector as a string representation.
    /// </summary>
    /// <returns>The CSS selector string.</returns>
    public override string ToString() => _selector;

    /// <summary>
    /// Generates inline CSS style string from this rule's properties.
    /// </summary>
    /// <returns>Inline CSS style string.</returns>
    public override string Inline()
    {
        var sb = new StringBuilder(GetSizeEstimation());

        WriteInline(this, sb);

        return sb.ToString();
    }

    /// <summary>
    /// Generates complete CSS string from this rule and all nested rules.
    /// </summary>
    /// <returns>Complete CSS string.</returns>
    public override string ToCss()
    {
        var sb = new StringBuilder(GetSizeEstimation());

        WriteCss(string.Empty, this, sb);

        return sb.ToString();
    }

    /// <summary>
    /// Adds a nested rule with the specified selector and configuration.
    /// </summary>
    /// <param name="selector">The CSS selector for the nested rule.</param>
    /// <param name="configure">Action to configure the nested rule.</param>
    /// <returns>The current CSS rule instance for method chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private CssRule AddRule(string selector, Action<CssRule> configure)
    {
        var rule = new CssRuleInternal(selector);
        configure(rule);
        _rules.Add(rule);

        return this;
    }

    /// <summary>
    /// Estimates the size of the generated CSS for StringBuilder capacity optimization.
    /// </summary>
    /// <returns>Estimated size in characters.</returns>
    private int GetSizeEstimation() => _properties.Count * 20 + _rules.Select(x => x.GetSizeEstimation()).Sum();
}
