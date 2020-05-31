using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace XRest
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Core.ServicePack>();
            Add<Clients.Dotnet.ServicePack>();
            Add<Clients.TypeScript.ServicePack>();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            RegisterCommands(services);

            services.AddArguments();
            services.AddLogging(route => route.UseConsole(time: true));
        }

        private void RegisterCommands(IServiceCollection services)
        {
            services.AddSingleton<Commands.Group>();
            services.AddSingleton<Commands.ParseCommand>();
        }
    }
}