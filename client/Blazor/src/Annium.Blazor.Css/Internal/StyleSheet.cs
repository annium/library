using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Linq;

namespace Annium.Blazor.Css.Internal;

internal class StyleSheet : IStyleSheet
{
    public static readonly StyleSheet Instance = new();
#if DEBUG
    private static readonly string _separator = Environment.NewLine;
#else
    private static readonly string _separator = string.Empty;
#endif

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

    public string Css => _css.Value;
    public event Action CssChanged = delegate { };
    private Lazy<string> _css;
    private readonly List<RuleSet> _ruleSets = new();

    private StyleSheet()
    {
        _css = new Lazy<string>(Render);
    }

    public void Add(RuleSet ruleSet)
    {
        _ruleSets.Add(ruleSet);
        _css = new Lazy<string>(Render);
        CssChanged.Invoke();
    }

    private string Render()
    {
        var result = _ruleSets.SelectMany(GetRules).Select(x => x.ToCss()).Join(_separator);
        return result;
    }
}
