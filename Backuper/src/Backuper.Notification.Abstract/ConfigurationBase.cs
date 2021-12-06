using Annium.Core.Runtime.Types;

namespace Backuper.Notification.Abstract;

public abstract class ConfigurationBase
{
    [ResolutionKey]
    public string Type { get; set; } = string.Empty;
}