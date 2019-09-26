using Annium.Core.Reflection;

namespace Backuper.Connection.PostgreSQL
{
    [ResolveKey("postgres")]
    public class Configuration : Abstract.ConfigurationBase
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Db { get; set; }

        public string User { get; set; }

        public string Pass { get; set; }
    }
}