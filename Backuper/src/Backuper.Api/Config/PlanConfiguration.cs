using System.Collections.Generic;

namespace Backuper.Api.Config
{
    public class PlanConfiguration
    {
        public Annium.Storage.Abstractions.ConfigurationBase Storage { get; set; }

        public string Interval { get; set; }

        public int Capacity { get; set; }

        public Dictionary<string, Notification.Abstract.ConfigurationBase> Notifications { get; set; }
    }
}