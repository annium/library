using Annium.Core.Reflection;

namespace Backuper.Connection.PostgreSQL
{
    [ResolveKey("postgres")]
    public class Configuration : Abstract.ConfigurationBase
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string Db { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        public string Pass { get; set; } = string.Empty;
    }
}