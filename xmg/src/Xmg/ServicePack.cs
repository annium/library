using System;
using Annium.Core.DependencyInjection;

namespace Xmg
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Configuration.ServicePack>();
            Add<Migration.ServicePack>();
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddTimeProvider();

            container.AddArguments();
            container.AddJsonSerializers();
            container.AddLogging(route => route.UseConsole());
        }
    }
}