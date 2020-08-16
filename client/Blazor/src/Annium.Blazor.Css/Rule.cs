using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css
{
    public static class Rule
    {
        private static int _index = 0;
        private static int Index => Interlocked.Increment(ref _index);

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
        public static CssRule Class([CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
            => new CssRuleInternal($"{string.Empty}{RuleType.Class}{GenerateName(line, file, member)}");

        public static CssRule TagClass(string tag, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
            => new CssRuleInternal($"{tag}{RuleType.Class}{GenerateName(line, file, member)}");

        public static CssRule Id([CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
            => new CssRuleInternal($"{string.Empty}{RuleType.Id}{GenerateName(line, file, member)}");

        public static CssRule TagId(string tag, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
            => new CssRuleInternal($"{tag}{RuleType.Id}{GenerateName(line, file, member)}");

        private static readonly IDictionary<Type, int> TypeRules = new Dictionary<Type, int>();
        private static string GenerateName(int line, string file, string member)
        {
            var fileName = Path.GetFileNameWithoutExtension(file).Replace(".razor", string.Empty);
            var memberName = member == ".ctor" ? "constructor" : member;

            return $"{fileName}_{memberName}_{line}_{Index}";
        }
#else
        public static CssRule Class()
            => new CssRuleInternal($"{string.Empty}{RuleType.Class}{GenerateName()}");

        public static CssRule TagClass(string tag)
            => new CssRuleInternal($"{tag}{RuleType.Class}{GenerateName()}");

        public static CssRule Id()
            => new CssRuleInternal($"{string.Empty}{RuleType.Id}{GenerateName()}");

        public static CssRule TagId(string tag)
            => new CssRuleInternal($"{tag}{RuleType.Id}{GenerateName()}");

        private static string GenerateName() => $"a{Index}";
#endif
    }
}