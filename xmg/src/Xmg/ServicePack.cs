using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Extensions.Arguments;
using Annium.Logging.Console;
using Annium.Logging.Shared;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;

namespace Xmg;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Configuration.ServicePack>();
        Add<Migration.ServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddMapper();
        container.AddArguments();
        container.AddSerializers().WithJson(isDefault: true);
        container.AddLogging();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}
