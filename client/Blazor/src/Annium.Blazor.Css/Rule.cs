using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css
{
    public static class Rule
    {
        public static CssRule Class(string name)
            => new CssRuleInternal($"{string.Empty}{RuleType.Class}{name}");

        public static CssRule Tag(string tag)
            => new CssRuleInternal(tag);

        public static CssRule TagClass(string tag, string name)
            => new CssRuleInternal($"{tag}{RuleType.Class}{name}");

        public static CssRule Id(string name)
            => new CssRuleInternal($"{string.Empty}{RuleType.Id}{name}");

        public static CssRule TagId(string tag, string name)
            => new CssRuleInternal($"{tag}{RuleType.Id}{name}");

        public static CssRule Custom(string selector)
            => new CssRuleInternal(selector);

        public static CssRule Media(string query)
            => new CssRuleInternal($"@media {query}");

#if DEBUG
        public static CssRule Class([CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
            => new CssRuleInternal($"{string.Empty}{RuleType.Class}{GenerateName(line, member)}");

        public static CssRule TagClass(string tag, [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
            => new CssRuleInternal($"{tag}{RuleType.Class}{GenerateName(line, member)}");

        public static CssRule Id([CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
            => new CssRuleInternal($"{string.Empty}{RuleType.Id}{GenerateName(line, member)}");

        public static CssRule TagId(string tag, [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
            => new CssRuleInternal($"{tag}{RuleType.Id}{GenerateName(line, member)}");

        private static readonly IDictionary<Type, int> TypeRules = new Dictionary<Type, int>();
        private static string GenerateName(int line, string member)
        {
            var st = new StackTrace();
            var frame = st.GetFrames()[2];
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            var typeName = type.FullName.Replace('.', '-');

            return $"{typeName}_{member}_{line}";
        }
#else
        public static CssRule Class()
            => new RuleInternal($"{string.Empty}{RuleType.Class}{GenerateName()}");

        public static CssRule TagClass(string tag)
            => new RuleInternal($"{tag}{RuleType.Class}{GenerateName()}");

        public static CssRule Id()
            => new RuleInternal($"{string.Empty}{RuleType.Id}{GenerateName()}");

        public static CssRule TagId(string tag)
            => new RuleInternal($"{tag}{RuleType.Id}{GenerateName()}");

        private static int _index = 0;
        private static string GenerateName()
        {
            var index = Interlocked.Increment(ref _index);

            return $"a{index}";
        }
#endif
    }
}