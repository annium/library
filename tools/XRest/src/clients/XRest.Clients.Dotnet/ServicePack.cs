using System;
using Annium.Core.DependencyInjection;
using XRest.Clients.Dotnet.Commands;
using XRest.Clients.Dotnet.Components;
using XRest.Clients.Dotnet.Components.Implementations;

namespace XRest.Clients.Dotnet;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // components
        container.Add<IProcessor, Processor>().Singleton();
        container.Add<IWriter, Writer>().Singleton();

        RegisterCommands(container);
    }

    private void RegisterCommands(IServiceContainer container)
    {
        container.Add<Group>().AsSelf().Singleton();
        container.Add<GenerateCommand>().AsSelf().Singleton();
    }
}