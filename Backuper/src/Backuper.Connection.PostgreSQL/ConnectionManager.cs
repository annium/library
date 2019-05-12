using System.Threading.Tasks;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;

namespace Backuper.Connection.PostgreSQL
{
    public class ConnectionManager : Abstract.ConnectionManager<Configuration>
    {
        private readonly IShell shell;

        private readonly ILogger<Connection> logger;

        public ConnectionManager(
            IShell shell,
            ILogger<Connection> logger
        )
        {
            this.shell = shell;
            this.logger = logger;
        }

        public override Task<Abstract.Connection> GetConnectionAsync(string name, Configuration configuration)
        {
            var connection = new Connection(name, configuration, shell, logger);

            return Task.FromResult<Abstract.Connection>(connection);
        }
    }
}