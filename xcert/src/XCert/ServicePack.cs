using System;
using Annium.Core.DependencyInjection;

namespace XCert
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddRuntimeTools(GetType().Assembly, true);
            container.AddTime().WithRealTime().SetDefault();
            container.AddLogging(route => route.UseConsole());
            container.AddMapper();
        }
    }
}