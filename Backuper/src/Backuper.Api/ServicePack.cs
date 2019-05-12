using System;
using System.IO;
using Annium.Extensions.Configuration;
using Annium.Extensions.DependencyInjection;
using Annium.Extensions.Mapper;
using Backuper.Api.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Api
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Connection.PostgreSQL.ServicePack>();
            Add<Notification.Slack.ServicePack>();
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
            services.AddSingleton<StateManager>();

            Mapper.AddConfiguration(ConfigureMapping());
        }

        public override void Setup(System.IServiceProvider provider)
        {
            try
            {
                provider.GetRequiredService<StateManager>().GetState().GetAwaiter().GetResult();
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