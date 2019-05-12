using Annium.Extensions.DependencyInjection;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Storage.Abstract
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            services.AddSingleton<StorageFactory>();

            services.AddConsole(new LoggerConfiguration(LogLevel.Trace));
        }
    }
}