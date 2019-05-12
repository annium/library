using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Notification.Slack
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            services.AddSingleton<Abstract.ChannelManager<Configuration>, ChannelManager>();
        }
    }
}