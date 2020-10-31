using System;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Arguments;
using Microsoft.Extensions.DependencyInjection;

namespace Xc
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddRuntimeTools(GetType().Assembly, true);
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            RegisterCommands(services);

            services.AddArguments();
            services.AddConfigurationBuilder();
            services.AddLogging(route => route.UseConsole());
            services.AddMapper();
        }

        private void RegisterCommands(IServiceCollection services)
        {
            services.AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<Group>()
                .AsSelf()
                .SingleInstance();
            services.AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<CommandBase>()
                .AsSelf()
                .SingleInstance();
        }
    }
}