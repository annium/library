using System;
using Annium.Core.DependencyInjection;
using Annium.XRest.Clients.Csharp.Commands;
using Annium.XRest.Clients.Csharp.Components.Writers;

namespace Annium.XRest.Clients.Csharp;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // components
        container.Add<Writer>().AsSelf().Singleton();
        container.Add<ClientWriter>().AsSelf().Singleton();
        container.Add<ModelWriter>().AsSelf().Singleton();
        container.Add<FileWriter>().AsSelf().Singleton();

        RegisterCommands(container);
    }

    private void RegisterCommands(IServiceContainer container)
    {
        container.Add<Group>().AsSelf().Singleton();
        container.Add<GenerateCommand>().AsSelf().Singleton();
    }
}
