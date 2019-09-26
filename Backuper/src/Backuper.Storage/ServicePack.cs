using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Storage
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            services.AddSingleton<StorageFactory>();

            services.AddLogging(route => route.UseConsole());
        }
    }
}