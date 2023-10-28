using System.Collections.Generic;

namespace Backuper.Api.Config;

public class Configuration
{
    public Dictionary<string, ServerConfiguration> Servers { get; set; } =
        new Dictionary<string, ServerConfiguration>();
}
