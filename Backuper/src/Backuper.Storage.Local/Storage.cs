using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.Local
{
    public class Storage : Abstract.Storage
    {
        private readonly Configuration cfg;

        public Storage(
            string name,
            Configuration cfg,
            ILogger logger
        ) : base(name, logger)
        {
            this.cfg = cfg;
        }

        public override Task SetupAsync()
        {
            logger.Debug($"Setup local storage {Name}");

            return Task.CompletedTask;
        }
    }
}