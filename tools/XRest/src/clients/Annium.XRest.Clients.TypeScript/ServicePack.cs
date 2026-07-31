using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.XRest.Clients.TypeScript.Commands;
using Annium.XRest.Clients.TypeScript.Components;
using Annium.XRest.Clients.TypeScript.Components.Implementations;

namespace Annium.XRest.Clients.TypeScript;

public class ServicePack : ServicePackBase
{
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        // components
        // container.Add<IProcessor, Processor>().Singleton();
        container.Add<IWriter, Writer>().Singleton();

        RegisterCommands(container);

        return Task.CompletedTask;
    }

    private void RegisterCommands(IServiceContainer container)
    {
        container.Add<Group>().AsSelf().Singleton();
        container.Add<GenerateCommand>().AsSelf().Singleton();
    }
}
