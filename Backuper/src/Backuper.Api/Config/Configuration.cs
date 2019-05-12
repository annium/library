using System.Collections.Generic;

namespace Backuper.Api.Config
{
    public class Configuration
    {
        public Dictionary<string, ServerConfiguration> Servers { get; set; } =
            new Dictionary<string, ServerConfiguration>();

        public Dictionary<string, Storage.Abstract.Configuration> Storages { get; set; } =
            new Dictionary<string, Storage.Abstract.Configuration>();

        public Dictionary<string, Notification.Abstract.Configuration> Notifications { get; set; } =
            new Dictionary<string, Notification.Abstract.Configuration>();
    }
}