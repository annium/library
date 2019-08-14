using System;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Backuper.Notification.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Notification.Slack
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            Func<IServiceProvider, Func<Configuration, IChannel>> factory =
                sp => configuration => new ChannelProxy(
                    new Channel(configuration),
                    configuration.Type,
                    sp.GetRequiredService<ILogger<Channel>>()
                );

            services.AddSingleton<Func<Configuration, IChannel>>(factory);
        }
    }
}