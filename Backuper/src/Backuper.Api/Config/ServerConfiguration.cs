using System.Collections.Generic;

namespace Backuper.Api.Config
{
    public class ServerConfiguration
    {
        public Connection.Abstract.Configuration Connection { get; set; }

        public Dictionary<string, PlanConfiguration> Plans { get; set; }
    }
}