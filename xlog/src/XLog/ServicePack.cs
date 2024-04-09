using System;
using Annium.Core.DependencyInjection;
using XLog.Components;
using XLog.Internal.Components;

namespace XLog;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddHttpRequestFactory(isDefault: true);
        container.AddSerializers().WithJson(isDefault: true).WithYaml(isDefault: true);
        container.AddMapper();
        container.AddLogging();
        container.AddArguments();

        // components
        container.Add<IConfigurationManager, ConfigurationManager>().Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole(color: true));
    }
}
