using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Annium.Blazor.Css.Internal
{
    internal class RuleInternal : IRule
    {
        private readonly string _selector;
        private readonly IList<RuleInternal> _rules = new List<RuleInternal>();
        private readonly IDictionary<string, string> _properties = new Dictionary<string, string>();

#if DEBUG
        private static string PropertyToCss(KeyValuePair<string, string> pair) => $"{pair.Key}: {pair.Value};";

        private static void WriteCss(RuleInternal rule, StringBuilder sb, int indent = 0)
        {
            var indentation = new string(' ', indent);
            var propertyIndentation = new string(' ', indent + 2);

            sb.AppendLine($"{indentation}{rule._selector} {{");

            foreach (var property in rule._properties.Select(PropertyToCss))
                sb.AppendLine($"{propertyIndentation}{property}");

            foreach (var innerRule in rule._rules)
            {
                sb.AppendLine();
                WriteCss(innerRule, sb, indent + 2);
            }

            sb.AppendLine($"{indentation}}}");
        }
#else
        private static string PropertyToCss(KeyValuePair<string, string> pair) => $"{pair.Key}:{pair.Value};";

        private static void WriteCss(RuleInternal rule, StringBuilder sb)
        {
            sb.Append($"{rule._selector}{{");

            foreach (var property in rule._properties.Select(PropertyToCss))
                sb.Append(property);

            foreach (var innerRule in rule._rules)
                WriteCss(innerRule, sb);

            sb.Append("}");
        }
#endif

        public RuleInternal(string selector)
        {
            _selector = selector;
        }

        public IRule Set(string property, string value)
        {
            _properties[property] = value;

            return this;
        }

        public IRule And(string selector, Action<IRule> configure) => AddRule($"&{selector}", configure);

#if DEBUG
        public IRule Child(string selector, Action<IRule> configure) => AddRule($"> {selector}", configure);
#else
        public IRule Child(string selector, Action<IRule> configure) => AddRule($">{selector}", configure);
#endif

        public IRule Inheritor(string selector, Action<IRule> configure) => AddRule(selector, configure);

        public override string ToString() => _selector;

        public string ToCss()
        {
            var sb = new StringBuilder(GetSizeEstimation());

            WriteCss(this, sb);

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IRule AddRule(string selector, Action<IRule> configure)
        {
            var rule = new RuleInternal(selector);
            configure(rule);
            _rules.Add(rule);

            return this;
        }

        private int GetSizeEstimation() => _properties.Count * 20 + _rules.Select(x => x.GetSizeEstimation()).Sum();
    }
}