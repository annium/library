using System;
using Annium.Core.DependencyInjection;

namespace Backuper.Notification.Abstract
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.Add<ChannelFactory>().Singleton();

            container.AddLogging(route => route.UseConsole());
        }
    }
}