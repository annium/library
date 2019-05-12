using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Connection.PostgreSQL
{
    public class Connection : Abstract.Connection
    {
        public Connection(
            string name,
            ILogger logger
        ) : base(name, logger)
        {

        }

        public override Task SetupAsync()
        {
            logger.Debug($"Setup PostgreSQL connection {Name}");

            return Task.CompletedTask;
        }
    }
}