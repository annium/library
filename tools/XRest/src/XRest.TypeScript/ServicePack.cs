using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using XRest.TypeScript.Commands;
using XRest.TypeScript.Components;
using XRest.TypeScript.Components.Implementations;

namespace XRest.TypeScript
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // components
            services.AddSingleton<IProcessor, Processor>();
            services.AddSingleton<IWriter, Writer>();

            RegisterCommands(services);
        }

        private void RegisterCommands(IServiceCollection services)
        {
            services.AddSingleton<Group>();
            services.AddSingleton<GenerateCommand>();
        }
    }
}