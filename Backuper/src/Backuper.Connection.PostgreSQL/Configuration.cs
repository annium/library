using Annium.Core.Runtime.Types;
using Backuper.Connection.Abstract;

namespace Backuper.Connection.PostgreSQL
{
    [ResolutionKeyValue("postgres")]
    public class Configuration : ConfigurationBase
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string Db { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        public string Pass { get; set; } = string.Empty;
    }
}