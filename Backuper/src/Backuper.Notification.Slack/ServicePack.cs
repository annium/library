using System;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Annium.Net.Http;
using Backuper.Notification.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Notification.Slack
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            Func<Configuration, IChannel> Factory(IServiceProvider sp) => configuration =>
                new ChannelProxy(new Channel(sp.GetRequiredService<IHttpRequestFactory>(), configuration), configuration.Type,
                    sp.GetRequiredService<ILogger<Channel>>());

            services.AddSingleton(Factory);
        }
    }
}