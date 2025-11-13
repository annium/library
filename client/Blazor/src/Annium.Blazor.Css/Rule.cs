using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css;

/// <summary>
/// Factory class for creating CSS rules with various selectors
/// </summary>
public static class Rule
{
    /// <summary>
    /// Gets the next unique index for rule generation
    /// </summary>
    private static int Index => Interlocked.Increment(ref field);

    /// <summary>
    /// Creates a CSS class rule with the specified name
    /// </summary>
    /// <param name="name">The class name</param>
    /// <returns>A new CSS top-level rule for the class</returns>
    public static CssTopLevelRule Class(string name) => new CssRuleInternal($"{string.Empty}{RuleType.Class}{name}");

    /// <summary>
    /// Creates a CSS tag rule for the specified HTML tag
    /// </summary>
    /// <param name="tag">The HTML tag name</param>
    /// <returns>A new CSS top-level rule for the tag</returns>
    public static CssTopLevelRule Tag(string tag) => new CssRuleInternal(tag);

    /// <summary>
    /// Creates a CSS rule for a specific tag with a class
    /// </summary>
    /// <param name="tag">The HTML tag name</param>
    /// <param name="name">The class name</param>
    /// <returns>A new CSS top-level rule for the tag with class</returns>
    public static CssTopLevelRule TagClass(string tag, string name) =>
        new CssRuleInternal($"{tag}{RuleType.Class}{name}");

    /// <summary>
    /// Creates a CSS ID rule with the specified name
    /// </summary>
    /// <param name="name">The ID name</param>
    /// <returns>A new CSS top-level rule for the ID</returns>
    public static CssTopLevelRule Id(string name) => new CssRuleInternal($"{string.Empty}{RuleType.Id}{name}");

    /// <summary>
    /// Creates a CSS rule for a specific tag with an ID
    /// </summary>
    /// <param name="tag">The HTML tag name</param>
    /// <param name="name">The ID name</param>
    /// <returns>A new CSS top-level rule for the tag with ID</returns>
    public static CssTopLevelRule TagId(string tag, string name) => new CssRuleInternal($"{tag}{RuleType.Id}{name}");

    /// <summary>
    /// Creates a CSS rule with a custom selector
    /// </summary>
    /// <param name="selector">The custom CSS selector</param>
    /// <returns>A new CSS top-level rule for the custom selector</returns>
    public static CssTopLevelRule Custom(string selector) => new CssRuleInternal(selector);

#if DEBUG
    /// <summary>
    /// Creates a CSS class rule with an auto-generated name based on caller information (DEBUG only)
    /// </summary>
    /// <param name="line">The line number where this method is called</param>
    /// <param name="file">The file path where this method is called</param>
    /// <param name="member">The member name where this method is called</param>
    /// <returns>A new CSS top-level rule for the auto-generated class</returns>
    public static CssTopLevelRule Class(
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = ""
    ) => new CssRuleInternal($"{string.Empty}{RuleType.Class}{GenerateName(line, file, member)}");

    /// <summary>
    /// Creates a CSS rule for a specific tag with an auto-generated class name based on caller information (DEBUG only)
    /// </summary>
    /// <param name="tag">The HTML tag name</param>
    /// <param name="line">The line number where this method is called</param>
    /// <param name="file">The file path where this method is called</param>
    /// <param name="member">The member name where this method is called</param>
    /// <returns>A new CSS top-level rule for the tag with auto-generated class</returns>
    public static CssTopLevelRule TagClass(
        string tag,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = ""
    ) => new CssRuleInternal($"{tag}{RuleType.Class}{GenerateName(line, file, member)}");

    /// <summary>
    /// Creates a CSS ID rule with an auto-generated name based on caller information (DEBUG only)
    /// </summary>
    /// <param name="line">The line number where this method is called</param>
    /// <param name="file">The file path where this method is called</param>
    /// <param name="member">The member name where this method is called</param>
    /// <returns>A new CSS top-level rule for the auto-generated ID</returns>
    public static CssTopLevelRule Id(
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = ""
    ) => new CssRuleInternal($"{string.Empty}{RuleType.Id}{GenerateName(line, file, member)}");

    /// <summary>
    /// Creates a CSS rule for a specific tag with an auto-generated ID name based on caller information (DEBUG only)
    /// </summary>
    /// <param name="tag">The HTML tag name</param>
    /// <param name="line">The line number where this method is called</param>
    /// <param name="file">The file path where this method is called</param>
    /// <param name="member">The member name where this method is called</param>
    /// <returns>A new CSS top-level rule for the tag with auto-generated ID</returns>
    public static CssTopLevelRule TagId(
        string tag,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = ""
    ) => new CssRuleInternal($"{tag}{RuleType.Id}{GenerateName(line, file, member)}");

    /// <summary>
    /// Generates a unique name based on caller information for DEBUG builds
    /// </summary>
    /// <param name="line">The line number</param>
    /// <param name="file">The file path</param>
    /// <param name="member">The member name</param>
    /// <returns>A unique generated name</returns>
    private static string GenerateName(int line, string file, string member)
    {
        var filePath = Regex
            .Replace(file, "(.razor|.cs|[:#.])", string.Empty)
            .Replace(':', '-')
            .TrimStart(Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar)
            .ToArray();
        var fileName = string.Join('-', filePath.Length > 4 ? filePath.TakeLast(4) : filePath);
        var memberName = member == ".ctor" ? "constructor" : member;

        return $"{fileName}_{memberName}_{line}_{Index}";
    }
#else
    /// <summary>
    /// Creates a CSS class rule with an auto-generated name (RELEASE only)
    /// </summary>
    /// <returns>A new CSS top-level rule for the auto-generated class</returns>
    public static CssTopLevelRule Class() => new CssRuleInternal($"{string.Empty}{RuleType.Class}{GenerateName()}");

    /// <summary>
    /// Creates a CSS rule for a specific tag with an auto-generated class name (RELEASE only)
    /// </summary>
    /// <param name="tag">The HTML tag name</param>
    /// <returns>A new CSS top-level rule for the tag with auto-generated class</returns>
    public static CssTopLevelRule TagClass(string tag) => new CssRuleInternal($"{tag}{RuleType.Class}{GenerateName()}");

    /// <summary>
    /// Creates a CSS ID rule with an auto-generated name (RELEASE only)
    /// </summary>
    /// <returns>A new CSS top-level rule for the auto-generated ID</returns>
    public static CssTopLevelRule Id() => new CssRuleInternal($"{string.Empty}{RuleType.Id}{GenerateName()}");

    /// <summary>
    /// Creates a CSS rule for a specific tag with an auto-generated ID name (RELEASE only)
    /// </summary>
    /// <param name="tag">The HTML tag name</param>
    /// <returns>A new CSS top-level rule for the tag with auto-generated ID</returns>
    public static CssTopLevelRule TagId(string tag) => new CssRuleInternal($"{tag}{RuleType.Id}{GenerateName()}");

    /// <summary>
    /// Generates a simple unique name for RELEASE builds
    /// </summary>
    /// <returns>A unique generated name</returns>
    private static string GenerateName() => $"a{Index}";
#endif
}
