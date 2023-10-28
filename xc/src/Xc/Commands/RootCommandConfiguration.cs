using System.IO;
using Annium.Extensions.Arguments;

namespace Xc.Commands;

internal class RootCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Path to configuration root.")]
    public string Path
    {
        get => _path;
        set => _path = System.IO.Path.GetFullPath(value);
    }

    private string _path = Directory.GetCurrentDirectory();
}
