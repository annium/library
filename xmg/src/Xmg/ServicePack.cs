using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xmg.Commands;
using Xmg.Components;
using Xmg.Components.Implementations;
using Xmg.Tools;
using Xmg.Tools.Implementations;

namespace Xmg
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);
            RegisterCommands(services);

            services.AddArguments();
            services.AddLogging(route => route.UseConsole(time: true));


            // components
            services.AddSingleton<ILoader, Loader>();

            // tools
            services.AddSingleton<ITemplateWriter, TemplateWriter>();

            // externals
            services.AddResourceLoader();
            services.AddLoadContextFactories();
        }

        private void RegisterCommands(IServiceCollection services)
        {
            services.AddSingleton<Group>();
            services.AddSingleton<GenerateCommand>();
        }
    }
}