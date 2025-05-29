using System;
using Annium.Core.DependencyInjection;
using Annium.XRest.Core;

namespace Annium.XRest;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Clients.Csharp.ServicePack>();
        Add<Clients.TypeScript.ServicePack>();
        Add<Clients.Shared.ServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddArguments();
        container.AddLogging();
        container.AddMapper();
        container.AddHttpRequestFactory(Constants.IndexKey);
        container.AddXRestSerializer();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}
