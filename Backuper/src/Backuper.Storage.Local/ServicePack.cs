using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Storage.Local
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            services.AddSingleton<Abstract.StorageManager<Configuration>, StorageManager>();
        }
    }
}