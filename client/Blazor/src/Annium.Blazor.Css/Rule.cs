using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css;

public static class Rule
{
    private static int _index;
    private static int Index => Interlocked.Increment(ref _index);

    public static CssTopLevelRule Class(string name)
        => new CssRuleInternal($"{string.Empty}{RuleType.Class}{name}");

    public static CssTopLevelRule Tag(string tag)
        => new CssRuleInternal(tag);

    public static CssTopLevelRule TagClass(string tag, string name)
        => new CssRuleInternal($"{tag}{RuleType.Class}{name}");

    public static CssTopLevelRule Id(string name)
        => new CssRuleInternal($"{string.Empty}{RuleType.Id}{name}");

    public static CssTopLevelRule TagId(string tag, string name)
        => new CssRuleInternal($"{tag}{RuleType.Id}{name}");

    public static CssTopLevelRule Custom(string selector)
        => new CssRuleInternal(selector);

#if DEBUG
    public static CssTopLevelRule Class([CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
        => new CssRuleInternal($"{string.Empty}{RuleType.Class}{GenerateName(line, file, member)}");

    public static CssTopLevelRule TagClass(string tag, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
        => new CssRuleInternal($"{tag}{RuleType.Class}{GenerateName(line, file, member)}");

    public static CssTopLevelRule Id([CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
        => new CssRuleInternal($"{string.Empty}{RuleType.Id}{GenerateName(line, file, member)}");

    public static CssTopLevelRule TagId(string tag, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
        => new CssRuleInternal($"{tag}{RuleType.Id}{GenerateName(line, file, member)}");

    private static string GenerateName(int line, string file, string member)
    {
        var filePath = Regex.Replace(file, "(.razor|.cs|[:#.])", string.Empty)
            .Replace(':', '-')
            .TrimStart(Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar)
            .ToArray();
        var fileName = string.Join('-', filePath.Length > 4 ? filePath.TakeLast(4) : filePath);
        var memberName = member == ".ctor" ? "constructor" : member;

        return $"{fileName}_{memberName}_{line}_{Index}";
    }
#else
        public static CssTopLevelRule Class()
            => new CssRuleInternal($"{string.Empty}{RuleType.Class}{GenerateName()}");

        public static CssTopLevelRule TagClass(string tag)
            => new CssRuleInternal($"{tag}{RuleType.Class}{GenerateName()}");

        public static CssTopLevelRule Id()
            => new CssRuleInternal($"{string.Empty}{RuleType.Id}{GenerateName()}");

        public static CssTopLevelRule TagId(string tag)
            => new CssRuleInternal($"{tag}{RuleType.Id}{GenerateName()}");

        private static string GenerateName() => $"a{Index}";
#endif
}