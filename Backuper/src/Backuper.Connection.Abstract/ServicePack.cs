using Annium.Extensions.DependencyInjection;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Connection.Abstract
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            services.AddSingleton<ConnectionFactory>();

            services.AddConsoleLogger(new LoggerConfiguration(LogLevel.Trace));
            services.AddShell();
        }
    }
}