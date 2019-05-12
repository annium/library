using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Storage.S3
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            services.AddSingleton<Abstract.StorageManager<Configuration>, StorageManager>();
        }
    }
}