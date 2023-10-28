using Annium.Core.Runtime.Types;

namespace Backuper.Connection.Abstract;

public abstract class ConfigurationBase
{
    [ResolutionKey]
    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
