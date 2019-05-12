using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Connection.PostgreSQL
{
    public class ConnectionManager : Abstract.ConnectionManager<Configuration>
    {
        private readonly ILogger<Connection> logger;

        public ConnectionManager(
            ILogger<Connection> logger
        )
        {
            this.logger = logger;
        }

        public override Task<Abstract.Connection> GetConnectionAsync(string name, Configuration configuration)
        {
            return Task.FromResult<Abstract.Connection>(new Connection(name, configuration, logger));
        }
    }
}