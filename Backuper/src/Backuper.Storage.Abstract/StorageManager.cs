using System.Threading.Tasks;

namespace Backuper.Storage.Abstract
{
    public abstract class StorageManager<T> where T : Configuration
    {
        public abstract Task<Storage> GetStorageAsync(T configuration);
    }
}