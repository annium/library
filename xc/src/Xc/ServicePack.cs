using System;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Arguments;
using Xc.Tasks;

namespace Xc
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceContainer container)
        {
            container.AddRuntimeTools(GetType().Assembly, true);
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            RegisterCommands(container);

            container.AddArguments();
            container.AddLogging(route => route.UseConsole());
            container.AddMapper();
        }

        private void RegisterCommands(IServiceContainer container)
        {
            container.AddAll(GetType().Assembly)
                .AssignableTo<Group>()
                .AsSelf()
                .Singleton();
            container.AddAll(GetType().Assembly)
                .AssignableTo<CommandBase>()
                .AsSelf()
                .Singleton();
            container.AddAll(GetType().Assembly)
                .AssignableTo<ITask>()
                .AsSelf()
                .AsSelfFactory()
                .Transient();
        }
    }
}