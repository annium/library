using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Linq;

namespace Annium.Blazor.Css.Internal;

/// <summary>
/// Internal implementation of stylesheet that manages CSS rule sets and generates CSS output.
/// </summary>
internal class StyleSheet : IStyleSheet
{
    /// <summary>
    /// Singleton instance of the StyleSheet.
    /// </summary>
    public static readonly StyleSheet Instance = new();

#if DEBUG
    /// <summary>
    /// Separator used between CSS rules in debug mode.
    /// </summary>
    private static readonly string _separator = Environment.NewLine;
#else
    /// <summary>
    /// Separator used between CSS rules in release mode.
    /// </summary>
    private static readonly string _separator = string.Empty;
#endif

    /// <summary>
    /// Extracts CSS rules from a rule set using reflection.
    /// </summary>
    /// <param name="set">The rule set to extract rules from.</param>
    /// <returns>Collection of CSS rules found in the rule set.</returns>
    private static IReadOnlyCollection<CssRule> GetRules(RuleSet set)
    {
        var rules = new List<CssRule>();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var fields = set.GetType().GetFields(flags).Where(x => x.FieldType.IsAssignableFrom(typeof(CssRule))).ToArray();
        foreach (var field in fields)
            rules.Add(
                (CssRule)field.GetValue(set)!
                    ?? throw new InvalidOperationException($"Field {field} contains empty rule")
            );

        return rules;
    }

    /// <summary>
    /// Gets the generated CSS string from all registered rule sets.
    /// </summary>
    public string Css => _css.Value;

    /// <summary>
    /// Event fired when the CSS content changes due to rule set additions.
    /// </summary>
    public event Action CssChanged = delegate { };

    /// <summary>
    /// Lazily evaluated CSS string to optimize performance.
    /// </summary>
    private Lazy<string> _css;

    /// <summary>
    /// Collection of registered rule sets.
    /// </summary>
    private readonly List<RuleSet> _ruleSets = new();

    /// <summary>
    /// Initializes a new instance of the StyleSheet class.
    /// </summary>
    private StyleSheet()
    {
        _css = new Lazy<string>(Render);
    }

    /// <summary>
    /// Adds a rule set to the stylesheet and triggers CSS regeneration.
    /// </summary>
    /// <param name="ruleSet">The rule set to add.</param>
    public void Add(RuleSet ruleSet)
    {
        _ruleSets.Add(ruleSet);
        _css = new Lazy<string>(Render);
        CssChanged.Invoke();
    }

    /// <summary>
    /// Renders all rule sets into a single CSS string.
    /// </summary>
    /// <returns>The complete CSS string.</returns>
    private string Render()
    {
        var result = _ruleSets.SelectMany(GetRules).Select(x => x.ToCss()).Join(_separator);
        return result;
    }
}
