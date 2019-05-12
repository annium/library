using System;
using System.IO;
using Annium.Extensions.Configuration;
using Annium.Extensions.DependencyInjection;
using Annium.Extensions.Mapper;
using Annium.Logging.Abstractions;
using Backuper.Api.Config;
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
            Add<Storage.Abstract.ServicePack>();
            Add<Storage.Local.ServicePack>();
            Add<Storage.S3.ServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .AddYamlFile(Path.Combine("configuration", "config.yml"))
                .Build<Configuration>();

            services.AddSingleton(configuration);
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);

            services.AddSingleton<State.StateFactory>();
            services.AddSingleton<State.StateManager>();

            Mapper.AddConfiguration(ConfigureMapping());
            services.AddScheduler();
            services.AddConsole(new LoggerConfiguration(LogLevel.Trace));
        }

        public override void Setup(System.IServiceProvider provider)
        {
            var stateFactory = provider.GetRequiredService<State.StateFactory>();
            var stateManager = provider.GetRequiredService<State.StateManager>();

            try
            {
                var state = stateFactory.GetState().GetAwaiter().GetResult();
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