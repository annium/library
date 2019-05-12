using Annium.Extensions.DependencyInjection;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Notification.Abstract
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            services.AddSingleton<ChannelFactory>();

            services.AddConsole(new LoggerConfiguration(LogLevel.Trace));
        }
    }
}