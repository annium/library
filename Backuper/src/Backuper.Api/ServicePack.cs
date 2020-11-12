using System;
using System.IO;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Backuper.Api.Config;
using Backuper.Api.State;
using Backuper.Api.Tools;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

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

        public override void Configure(IServiceCollection services)
        {
            services
                .AddStorage()
                .AddFileSystemStorage()
                .AddS3Storage();

            services.AddConfiguration<Configuration>(x => x.AddYamlFile(Path.Combine("configuration", "config.yml")));
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);

            services.AddSingleton<StateFactory>();
            services.AddSingleton<StateManager>();
            services.AddSingleton<Func<State.State>>(sp => () => sp.GetRequiredService<StateManager>().State!);
            services.AddSingleton<Namer>();

            services.AddScheduler();
            services.AddMediator();
            services.AddLogging(route => route.UseConsole());
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