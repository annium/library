using System.Collections.Generic;
using Annium.Storage.Abstractions;

namespace Backuper.Api.Config;

public class PlanConfiguration
{
    public ConfigurationBase Storage { get; set; } = default!;

    public string Interval { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public Dictionary<string, Notification.Abstract.ConfigurationBase> Notifications { get; set; } =
        new Dictionary<string, Notification.Abstract.ConfigurationBase>();
}
