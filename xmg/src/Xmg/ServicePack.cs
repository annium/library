using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xmg.Commands;

namespace Xmg
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Configuration.ServicePack>();
            Add<Migration.ServicePack>();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);
            RegisterCommands(services);

            services.AddArguments();
            services.AddLogging(route => route.UseConsole(time: true));
        }

        private void RegisterCommands(IServiceCollection services)
        {
            services.AddSingleton<Group>();
            services.AddSingleton<GenerateCommand>();
        }
    }
}