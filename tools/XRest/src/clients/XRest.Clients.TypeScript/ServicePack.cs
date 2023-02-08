using System;
using Annium.Core.DependencyInjection;
using XRest.Clients.TypeScript.Commands;
using XRest.Clients.TypeScript.Components;
using XRest.Clients.TypeScript.Components.Implementations;

namespace XRest.Clients.TypeScript;

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