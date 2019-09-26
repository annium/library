using Annium.Core.Reflection;

namespace Backuper.Notification.Slack
{
    [ResolveKey("slack")]
    public class Configuration : Abstract.ConfigurationBase
    {
        public string Team { get; set; }

        public string Channel { get; set; }

        public string Token { get; set; }
    }
}