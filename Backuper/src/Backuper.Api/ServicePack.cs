using System;
using System.IO;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Logging.Abstractions;
using Backuper.Api.Config;
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

            var configuration = new ConfigurationBuilder()
                .AddYamlFile(Path.Combine("configuration", "config.yml"))
                .Build<Configuration>();

            services.AddSingleton(configuration);

            services.AddSingleton(new LoggerConfiguration(LogLevel.Trace));
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);

            services.AddSingleton<State.StateFactory>();
            services.AddSingleton<State.StateManager>();
            services.AddSingleton<Func<State.State>>(sp => () => sp.GetRequiredService<State.StateManager>().State);
            services.AddSingleton<Namer>();

            Mapper.AddConfiguration(ConfigureMapping());
            services.AddScheduler();
            services.AddConsoleLogger();
        }

        public override void Setup(System.IServiceProvider provider)
        {
            var stateFactory = provider.GetRequiredService<State.StateFactory>();
            var stateManager = provider.GetRequiredService<State.StateManager>();

            try
            {
                var state = stateFactory.GetState();
                stateManager.SetState(state);
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        private MapperConfiguration ConfigureMapping()
        {
            var cfg = new MapperConfiguration();

            return cfg;
        }
    }
}