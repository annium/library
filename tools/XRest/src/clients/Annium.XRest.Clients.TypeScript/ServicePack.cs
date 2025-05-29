using System;
using Annium.Core.DependencyInjection;
using Annium.XRest.Clients.TypeScript.Commands;
using Annium.XRest.Clients.TypeScript.Components;
using Annium.XRest.Clients.TypeScript.Components.Implementations;

namespace Annium.XRest.Clients.TypeScript;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // components
        // container.Add<IProcessor, Processor>().Singleton();
        container.Add<IWriter, Writer>().Singleton();

        RegisterCommands(container);
    }

    private void RegisterCommands(IServiceContainer container)
    {
        container.Add<Group>().AsSelf().Singleton();
        container.Add<GenerateCommand>().AsSelf().Singleton();
    }
}
