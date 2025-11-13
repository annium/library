using System.IO;
using Annium.Extensions.Arguments;

namespace Xc.Commands;

internal class RootCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Path to configuration root.")]
    public string Path
    {
        get;
        set => field = System.IO.Path.GetFullPath(value);
    } = Directory.GetCurrentDirectory();
}
