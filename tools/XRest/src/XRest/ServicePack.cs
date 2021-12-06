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

    public override void Configure(IServiceContainer container)
    {
        container.AddRuntimeTools(GetType().Assembly, true);
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddArguments();
        container.AddLogging(route => route.UseConsole());
        container.AddMapper();
    }
}