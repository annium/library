using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Connection.PostgreSQL
{
    public class Connection : Abstract.Connection
    {
        private readonly Configuration cfg;

        public Connection(
            string name,
            Configuration cfg,
            ILogger logger
        ) : base(name, logger)
        {
            this.cfg = cfg;
        }

        public override Task SetupAsync()
        {
            logger.Debug($"Setup PostgreSQL connection {Name}");

            return Task.CompletedTask;
        }
    }
}