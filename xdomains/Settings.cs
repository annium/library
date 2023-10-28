using System.IO;
using System.Reflection;

namespace xdomains;

internal class Settings
{
    public string Root { get; }

    public Settings()
    {
        var assemlyLocation = Assembly.GetExecutingAssembly().Location;
        Root = Path.GetDirectoryName(assemlyLocation)!;
    }

    public string RootedPath(params string[] paths) => Path.Combine(Root, Path.Combine(paths));
}
