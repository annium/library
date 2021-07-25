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

        public override void Configure(IServiceContainer container)
        {
            container.AddRuntimeTools(GetType().Assembly, true);
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddTimeProvider();
            container.AddMapper();
            container.AddArguments();
            container.AddJsonSerializers().SetDefault();
            container.AddLogging(route => route.UseConsole());
        }
    }
}