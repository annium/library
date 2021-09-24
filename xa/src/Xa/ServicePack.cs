using System;
using Annium.Core.DependencyInjection;

namespace Xa
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddRuntimeTools(GetType().Assembly, true);
            container.AddTimeProvider();
            container.AddArguments();
            container.AddLogging(route => route.UseConsole());
            container.AddMapper();
        }
    }
}