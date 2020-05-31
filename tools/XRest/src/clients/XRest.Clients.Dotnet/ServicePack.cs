using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using XRest.Clients.Dotnet.Commands;
using XRest.Clients.Dotnet.Components;
using XRest.Clients.Dotnet.Components.Implementations;

namespace XRest.Clients.Dotnet
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