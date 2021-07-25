using System;
using Annium.Core.DependencyInjection;

namespace XLog
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddRuntimeTools(GetType().Assembly, false);
            container.AddTimeProvider();
            container.AddMapper();
            container.AddLogging(route => route.UseConsole(color: true));
            container.AddArguments();
        }
    }
}