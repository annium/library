using System;
using Annium.Core.DependencyInjection;
using XRest.Commands;

namespace XRest
{
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
            RegisterCommands(container);

            container.AddArguments();
            container.AddLogging(route => route.UseConsole());
            container.AddMapper();
        }

        private void RegisterCommands(IServiceContainer container)
        {
            container.Add<Group>().AsSelf().Singleton();
            container.Add<ParseCommand>().AsSelf().Singleton();
        }
    }
}