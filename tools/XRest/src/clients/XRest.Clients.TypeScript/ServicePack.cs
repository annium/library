using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using XRest.Clients.TypeScript.Commands;
using XRest.Clients.TypeScript.Components;
using XRest.Clients.TypeScript.Components.Implementations;

namespace XRest.Clients.TypeScript
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