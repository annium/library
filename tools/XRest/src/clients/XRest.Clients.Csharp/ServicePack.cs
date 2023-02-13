using System;
using Annium.Core.DependencyInjection;
using XRest.Clients.Csharp.Commands;
using XRest.Clients.Csharp.Components;
using XRest.Clients.Csharp.Components.Implementations;

namespace XRest.Clients.Csharp;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // components
        container.Add<IWriter, Writer>().Singleton();

        RegisterCommands(container);
    }

    private void RegisterCommands(IServiceContainer container)
    {
        container.Add<Group>().AsSelf().Singleton();
        container.Add<GenerateCommand>().AsSelf().Singleton();
    }
}