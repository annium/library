using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Logging.Console;
using Annium.Logging.Shared;

namespace XCert
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddRuntime(GetType().Assembly);
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
