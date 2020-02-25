using System;
using System.Reflection;
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

        public IChannel CreateChannel(ConfigurationBase configuration)
        {
            var factoryType = typeof(Func<,>).MakeGenericType(configuration.GetType(), typeof(IChannel));

            var factory = (Delegate) provider.GetRequiredService(factoryType);

            try
            {
                var storage = (IChannel) factory.DynamicInvoke(configuration)!;

                return storage;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        }
    }
}