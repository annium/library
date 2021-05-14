using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Blazor.Css.Internal
{
    internal class StyleSheet : IStyleSheet
    {
        public static readonly StyleSheet Instance = new();

        private static IReadOnlyCollection<CssRule> GetRules(RuleSet set)
        {
            var rules = new List<CssRule>();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var properties = set.GetType().GetProperties(flags)
                .Where(x => x.CanRead && x.PropertyType.IsAssignableFrom(typeof(CssRule)))
                .ToArray();
            foreach (var property in properties)
                rules.Add(
                    (CssRule) property.GetMethod!.Invoke(set, Array.Empty<object>())! ??
                    throw new InvalidOperationException($"Property {property} contains empty rule")
                );

            var fields = set.GetType().GetFields(flags)
                .Where(x => x.FieldType.IsAssignableFrom(typeof(CssRule)))
                .ToArray();
            foreach (var field in fields)
                rules.Add(
                    (CssRule) field.GetValue(set)! ??
                    throw new InvalidOperationException($"Field {field} contains empty rule")
                );

            return rules;
        }

        public string Css { get; private set; } = string.Empty;
        public event Action CssChanged = delegate { };

        private StyleSheet()
        {
        }

        public void Render(RuleSet ruleSet)
        {
#if DEBUG
            var separator = Environment.NewLine;
#else
            var separator = string.Empty;
#endif
            Css += string.Join(separator, GetRules(ruleSet).Select(x => x.ToCss()));
            CssChanged.Invoke();
        }
    }
}