using System.Collections.Generic;

namespace Backuper.Api.Config;

public class ServerConfiguration
{
    public Connection.Abstract.ConfigurationBase Connection { get; set; } = default!;

    public Dictionary<string, PlanConfiguration> Plans { get; set; } = new Dictionary<string, PlanConfiguration>();
}