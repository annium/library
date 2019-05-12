using System.Threading.Tasks;

namespace Backuper.Storage.Local
{
    public class StorageManager : Abstract.StorageManager<Configuration>
    {
        public override Task<Abstract.Storage> GetStorageAsync(Configuration configuration)
        {
            throw new System.NotImplementedException();
        }
    }
}