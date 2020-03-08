using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using xrest.Commands;

namespace xrest
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);

            services.AddSingleton<Tools.Generator>();
            services.AddSingleton<Tools.Parser>();
            services.AddSingleton<Tools.Writer>();

            RegisterCommands(services);

            services.AddArguments();
            services.AddLogging(route => route.UseConsole(time: true, color: true));
            services.AddShell();
        }

        private void RegisterCommands(IServiceCollection services)
        {
            services.AddSingleton<Group>();
            services.AddSingleton<GenerateCommand>();
        }
    }
}