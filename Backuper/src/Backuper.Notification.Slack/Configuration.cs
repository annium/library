using Annium.Core.Runtime.Types;
using Backuper.Notification.Abstract;

namespace Backuper.Notification.Slack;

[ResolutionKeyValue("slack")]
public class Configuration : ConfigurationBase
{
    public string Team { get; set; } = string.Empty;

    public string Channel { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}
