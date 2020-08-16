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
        public static IRule Class(string name)
            => new RuleInternal($"{string.Empty}{RuleType.Class}{name}");

        public static IRule Tag(string tag)
            => new RuleInternal(tag);

        public static IRule TagClass(string tag, string name)
            => new RuleInternal($"{tag}{RuleType.Class}{name}");

        public static IRule Id(string name)
            => new RuleInternal($"{string.Empty}{RuleType.Id}{name}");

        public static IRule TagId(string tag, string name)
            => new RuleInternal($"{tag}{RuleType.Id}{name}");

        public static IRule Custom(string selector)
            => new RuleInternal(selector);

        public static IRule Media(string query)
            => new RuleInternal($"@media {query}");

#if DEBUG
        public static IRule Class([CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
            => new RuleInternal($"{string.Empty}{RuleType.Class}{GenerateName(line, member)}");

        public static IRule TagClass(string tag, [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
            => new RuleInternal($"{tag}{RuleType.Class}{GenerateName(line, member)}");

        public static IRule Id([CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
            => new RuleInternal($"{string.Empty}{RuleType.Id}{GenerateName(line, member)}");

        public static IRule TagId(string tag, [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
            => new RuleInternal($"{tag}{RuleType.Id}{GenerateName(line, member)}");

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
        public static IRule Class()
            => new RuleInternal($"{string.Empty}{RuleType.Class}{GenerateName()}");

        public static IRule TagClass(string tag)
            => new RuleInternal($"{tag}{RuleType.Class}{GenerateName()}");

        public static IRule Id()
            => new RuleInternal($"{string.Empty}{RuleType.Id}{GenerateName()}");

        public static IRule TagId(string tag)
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