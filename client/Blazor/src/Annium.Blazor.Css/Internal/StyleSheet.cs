using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Annium.Blazor.Css.Internal
{
    internal class StyleSheet : IStyleSheet
    {
        private static IReadOnlyCollection<IRule> GetRules(IRuleSet set)
        {
            var rules = new List<IRule>();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var properties = set.GetType().GetProperties(flags)
                .Where(x => x.PropertyType.IsAssignableFrom(typeof(IRule)))
                .ToArray();
            foreach (var property in properties)
                rules.Add((IRule) property.GetMethod.Invoke(set, Array.Empty<object>()));

            var fields = set.GetType().GetFields(flags)
                .Where(x => x.FieldType.IsAssignableFrom(typeof(IRule)))
                .ToArray();
            foreach (var field in fields)
                rules.Add((IRule) field.GetValue(set));

            return rules;
        }

        private readonly IReadOnlyCollection<IRuleSet> _ruleSets;

        public StyleSheet(IEnumerable<IRuleSet> ruleSets)
        {
            _ruleSets = ruleSets.ToArray();
        }

        public string ToCss()
        {
            var sb = new StringBuilder();

#if DEBUG
            var separator = Environment.NewLine;
#else
            var separator = string.Empty;
#endif
            sb.AppendJoin(separator, _ruleSets.SelectMany(GetRules).Select(x => x.ToCss()));

            return sb.ToString();
        }
    }
}