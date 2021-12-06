using System.IO;

namespace Xc;

internal static class Helper
{
    public static string ConfigPath(string path) => Path.Combine(path, ".xc");
    public static string FilePath(string sources, string name) => Path.Combine(sources, "files", name);
    public static string VarsPath(string sources) => Path.Combine(sources, "vars", "vars.yml");
    public static string VarsPath(string sources, string env) => Path.Combine(sources, "files", $"vars.{env}.yml");
}