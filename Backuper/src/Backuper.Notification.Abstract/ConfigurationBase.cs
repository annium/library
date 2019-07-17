using Annium.Core.Application.Types;

namespace Backuper.Notification.Abstract
{
    public abstract class ConfigurationBase
    {
        [ResolveField]
        public string Type { get; set; }
    }
}