using System.Collections.Generic;
using Backuper.Connection.Abstract;

namespace Backuper.Api.Config;

public class ServerConfiguration
{
    public ConfigurationBase Connection { get; set; } = default!;

    public Dictionary<string, PlanConfiguration> Plans { get; set; } = new Dictionary<string, PlanConfiguration>();
}
