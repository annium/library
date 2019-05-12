using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Notification.Abstract
{
    public class ChannelFactory
    {
        private readonly IServiceProvider provider;

        public ChannelFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public async Task<Channel> GetChannelAsync(Configuration cfg)
        {
            var managerType = typeof(ChannelManager<>).MakeGenericType(cfg.GetType());

            var manager = provider.GetRequiredService(managerType);
            var getStorageAsync = managerType.GetMethod(nameof(ChannelManager<Configuration>.GetChannelAsync));

            try
            {
                var storage = (Channel) await ((Task<Channel>) getStorageAsync.Invoke(manager, new [] { cfg }));

                return storage;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}