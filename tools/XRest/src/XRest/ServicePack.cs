using System;
using Annium.Core.DependencyInjection;

namespace XRest;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Core.ServicePack>();
        Add<Clients.Dotnet.ServicePack>();
        Add<Clients.TypeScript.ServicePack>();
        Add<Sources.ServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntimeTools(GetType().Assembly, true);
        container.AddArguments();
        container.AddLogging();
        container.AddMapper();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}