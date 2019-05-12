using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Storage.Abstract
{
    public class StorageFactory
    {
        private readonly IServiceProvider provider;

        public StorageFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public async Task<Storage> GetStorageAsync(string name, Configuration cfg)
        {
            var managerType = typeof(StorageManager<>).MakeGenericType(cfg.GetType());

            var manager = provider.GetRequiredService(managerType);
            var getStorageAsync = managerType.GetMethod(nameof(StorageManager<Configuration>.GetStorageAsync));

            try
            {
                var storage = (Storage) await ((Task<Storage>) getStorageAsync.Invoke(manager, new object[] { name, cfg }));

                return storage;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}