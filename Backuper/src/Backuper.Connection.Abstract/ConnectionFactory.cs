using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Connection.Abstract
{
    public class ConnectionFactory
    {
        private readonly IServiceProvider provider;

        public ConnectionFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public async Task<Connection> GetConnectionAsync(string name, Configuration cfg)
        {
            var managerType = typeof(ConnectionManager<>).MakeGenericType(cfg.GetType());

            var manager = provider.GetRequiredService(managerType);
            var getStorageAsync = managerType.GetMethod(nameof(ConnectionManager<Configuration>.GetConnectionAsync));

            try
            {
                var storage = (Connection) await ((Task<Connection>) getStorageAsync.Invoke(manager, new object[] { name, cfg }));

                return storage;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}