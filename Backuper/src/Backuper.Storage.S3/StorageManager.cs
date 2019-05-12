using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.S3
{
    public class StorageManager : Abstract.StorageManager<Configuration>
    {
        private readonly ILogger<Storage> logger;

        public StorageManager(
            ILogger<Storage> logger
        )
        {
            this.logger = logger;
        }

        public override Task<Abstract.Storage> GetStorageAsync(string name, Configuration configuration)
        {
            return Task.FromResult<Abstract.Storage>(new Storage(name, logger));
        }
    }
}