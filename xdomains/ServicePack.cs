using System;
using Annium.Extensions.DependencyInjection;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using xdomains.Tools;

namespace xdomains
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);

            RegisterCommands(services);

            services.AddSingleton<Cache>();
            services.AddSingleton<Parser>();
            services.AddSingleton<Resolver>();
            services.AddSingleton<Worker>();

            services.AddSingleton<Settings>();

            services.AddConsole(new LoggerConfiguration(LogLevel.Debug));
        }

        private void RegisterCommands(IServiceCollection services)
        {
            services.AddSingleton<Commands.Group>();
            services.AddSingleton<Commands.CleanupCommand>();
            services.AddSingleton<Commands.QueryCommand>();
        }
    }
}