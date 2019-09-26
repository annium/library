using Annium.Core.Reflection;

namespace Backuper.Notification.Abstract
{
    public abstract class ConfigurationBase
    {
        [ResolveField]
        public string Type { get; set; }
    }
}