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
            container.AddLogging();
            container.AddMapper();
        }

        public override void Setup(IServiceProvider provider)
        {
            provider.UseLogging(route => route.UseConsole());
        }
    }
}