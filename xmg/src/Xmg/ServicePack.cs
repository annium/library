using System;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Arguments;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

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
            services.AddLogging(route => route.UseConsole());
        }

        private void RegisterCommands(IServiceCollection services)
        {
            services.AddAssemblyTypes(GetType().Assembly)
                .Where(x => typeof(Group).IsAssignableFrom(x))
                .SingleInstance();
            services.AddAssemblyTypes(GetType().Assembly)
                .Where(x => typeof(CommandBase).IsAssignableFrom(x))
                .SingleInstance();
        }
    }
}