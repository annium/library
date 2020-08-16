using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Annium.Blazor.Css.Internal
{
    internal class CssRuleInternal : CssRule
    {
        private readonly string _selector;
        private readonly IList<CssRuleInternal> _rules = new List<CssRuleInternal>();
        private readonly IDictionary<string, string> _properties = new Dictionary<string, string>();

#if DEBUG
        private static string PropertyToCss(KeyValuePair<string, string> pair) => $"{pair.Key}: {pair.Value};";

        private static void WriteCss(CssRuleInternal cssRule, StringBuilder sb, int indent = 0)
        {
            var indentation = new string(' ', indent);
            var propertyIndentation = new string(' ', indent + 2);

            sb.AppendLine($"{indentation}{cssRule._selector} {{");

            foreach (var property in cssRule._properties.Select(PropertyToCss))
                sb.AppendLine($"{propertyIndentation}{property}");

            foreach (var innerRule in cssRule._rules)
            {
                sb.AppendLine();
                WriteCss(innerRule, sb, indent + 2);
            }

            sb.AppendLine($"{indentation}}}");
        }
#else
        private static string PropertyToCss(KeyValuePair<string, string> pair) => $"{pair.Key}:{pair.Value};";

        private static void WriteCss(CssRuleInternal rule, StringBuilder sb)
        {
            sb.Append($"{rule._selector}{{");

            foreach (var property in rule._properties.Select(PropertyToCss))
                sb.Append(property);

            foreach (var innerRule in rule._rules)
                WriteCss(innerRule, sb);

            sb.Append("}");
        }
#endif

        public CssRuleInternal(string selector)
        {
            _selector = selector;
        }

        public override CssRule Set(string property, string value)
        {
            _properties[property] = value;

            return this;
        }

        public override CssRule And(string selector, Action<CssRule> configure) => AddRule($"&{selector}", configure);

#if DEBUG
        public override CssRule Child(string selector, Action<CssRule> configure) => AddRule($"> {selector}", configure);
#else
        public override CssRule Child(string selector, Action<CssRule> configure) => AddRule($">{selector}", configure);
#endif

        public override CssRule Inheritor(string selector, Action<CssRule> configure) => AddRule(selector, configure);

        public override string ToString() => _selector;

        public override string ToCss()
        {
            var sb = new StringBuilder(GetSizeEstimation());

            WriteCss(this, sb);

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
}