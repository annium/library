using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Annium.Blazor.Css.Internal;

internal class CssRuleInternal : CssTopLevelRule
{
    private readonly string _selector;
    private readonly IList<CssRuleInternal> _rules = new List<CssRuleInternal>();
    private readonly IDictionary<string, CssRuleInternal> _media = new Dictionary<string, CssRuleInternal>();
    private readonly IDictionary<string, string> _properties = new Dictionary<string, string>();

#if DEBUG
    private const int Indent = 2;
    private static string PropertyToCss(KeyValuePair<string, string> pair) => $"{pair.Key}: {pair.Value};";

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
    private static string PropertyToCss(KeyValuePair<string, string> pair) => $"{pair.Key}:{pair.Value};";

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

    private static void WriteInline(CssRuleInternal rule, StringBuilder sb)
    {
        // inner rules and media queries are forbidden
        if (rule._rules.Count != 0 || rule._media.Count != 0)
            throw new InvalidOperationException("Inner rules and media queries are not allowed in inline rules");

        foreach (var property in rule._properties.Select(PropertyToCss))
            sb.Append(property);
    }

    public CssRuleInternal(string selector)
    {
        Name = selector.Split('#', '.').Last();
        _selector = selector;
    }

    public override CssRule Set(string property, string value)
    {
        _properties[property] = value;

        return this;
    }

    public override CssRule And(string selector, Action<CssRule> configure) => AddRule(selector, configure);

#if DEBUG
    public override CssRule Child(string selector, Action<CssRule> configure) => AddRule($" > {selector}", configure);
#else
        public override CssRule Child(string selector, Action<CssRule> configure) => AddRule($">{selector}", configure);
#endif

    public override CssRule Inheritor(string selector, Action<CssRule> configure) => AddRule($" {selector}", configure);

    public override CssTopLevelRule Media(string query, Action<CssRule> configure)
    {
        var rule = new CssRuleInternal(_selector);
        configure(rule);
        _media[$"@media {query}"] = rule;

        return this;
    }

    public override string ToString() => _selector;

    public override string Inline()
    {
        var sb = new StringBuilder(GetSizeEstimation());

        WriteInline(this, sb);

        return sb.ToString();
    }

    public override string ToCss()
    {
        var sb = new StringBuilder(GetSizeEstimation());

        WriteCss(string.Empty, this, sb);

        return sb.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private CssRule AddRule(string selector, Action<CssRule> configure)
    {
        var rule = new CssRuleInternal(selector);
        configure(rule);
        _rules.Add(rule);

        return this;
    }

    private int GetSizeEstimation() => _properties.Count * 20 + _rules.Select(x => x.GetSizeEstimation()).Sum();
}