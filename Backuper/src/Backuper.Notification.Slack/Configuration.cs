using Annium.Core.Reflection;

namespace Backuper.Notification.Slack
{
    [ResolveKey("slack")]
    public class Configuration : Abstract.ConfigurationBase
    {
        public string Team { get; set; } = string.Empty;

        public string Channel { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
    }
}