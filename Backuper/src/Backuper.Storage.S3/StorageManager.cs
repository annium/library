using System.Threading.Tasks;

namespace Backuper.Storage.S3
{
    public class StorageManager : Abstract.StorageManager<Configuration>
    {
        public override Task<Abstract.Storage> GetStorageAsync(Configuration configuration)
        {
            return Task.FromResult<Abstract.Storage>(new Storage());
        }
    }
}