using System;
using Annium.Core.DependencyInjection;
using xdomains.Commands;
using xdomains.Tools;

namespace xdomains
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddTimeProvider();

            RegisterCommands(container);

            container.Add<Cache>().Singleton();
            container.Add<Parser>().Singleton();
            container.Add<Resolver>().Singleton();
            container.Add<Worker>().Singleton();

            container.Add<Settings>().Singleton();

            container.AddArguments();
            container.AddLogging(route => route.UseConsole());
        }

        private void RegisterCommands(IServiceContainer container)
        {
            container.Add<Group>().Singleton();
            container.Add<CleanupCommand>().Singleton();
            container.Add<QueryCommand>().Singleton();
        }
    }
}