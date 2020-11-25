using System;
using System.IO;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Backuper.Api.Config;
using Backuper.Api.State;
using Backuper.Api.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Api
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Connection.Abstract.ServicePack>();
            Add<Connection.PostgreSQL.ServicePack>();
            Add<Notification.Abstract.ServicePack>();
            Add<Notification.Slack.ServicePack>();
            Add<Storage.ServicePack>();
        }

        public override void Configure(IServiceContainer container)
        {
            container
                .AddStorage()
                .AddFileSystemStorage()
                .AddS3Storage();

            container.AddConfiguration<Configuration>(x => x.AddYamlFile(Path.Combine("configuration", "config.yml")));
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddTimeProvider();

            container.Add<StateFactory>().AsSelf().Singleton();
            container.Add<StateManager>().AsSelf().Singleton();
            container.Add<Func<State.State>>(sp => () => sp.GetRequiredService<StateManager>().State!).Singleton();
            container.Add<Namer>().AsSelf().Singleton();

            container.AddScheduler();
            container.AddMediator();
            container.AddLogging(route => route.UseConsole());
        }

        public override void Setup(IServiceProvider provider)
        {
            var stateFactory = provider.GetRequiredService<StateFactory>();
            var stateManager = provider.GetRequiredService<StateManager>();

            try
            {
                var state = stateFactory.GetState();
                stateManager.SetState(state);
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException!;
            }
        }
    }
}