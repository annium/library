using System.Collections.Generic;
using Backuper.Notification.Abstract;

namespace Backuper.Api.Config;

public class PlanConfiguration
{
    // public ConfigurationBase Storage { get; set; } = default!;

    public string Interval { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public Dictionary<string, ConfigurationBase> Notifications { get; set; } =
        new Dictionary<string, ConfigurationBase>();
}
