using Annium.Extensions.Mapper;

namespace Backuper.Notification.Abstract
{
    public abstract class Configuration
    {
        [ResolveField]
        public string Type { get; set; }
    }
}