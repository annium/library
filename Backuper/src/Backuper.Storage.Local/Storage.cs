using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.Local
{
    public class Storage : Abstract.Storage
    {
        public Storage(
            string name,
            ILogger logger
        ) : base(name, logger)
        {

        }

        public override Task SetupAsync()
        {
            logger.Debug($"Setup local storage {Name}");

            return Task.CompletedTask;
        }
    }
}