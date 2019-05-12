using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.S3
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
            logger.Debug($"Setup S3 storage {Name}");

            return Task.CompletedTask;
        }
    }
}