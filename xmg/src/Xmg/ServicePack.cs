using System;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Arguments;

namespace Xmg
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Configuration.ServicePack>();
            Add<Migration.ServicePack>();
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddTimeProvider();
            RegisterCommands(container);

            container.AddArguments();
            container.AddJsonSerializers();
            container.AddLogging(route => route.UseConsole());
        }

        private void RegisterCommands(IServiceContainer container)
        {
            container.AddAll(GetType().Assembly)
                .Where(x => typeof(Group).IsAssignableFrom(x))
                .Singleton();
            container.AddAll(GetType().Assembly)
                .Where(x => typeof(CommandBase).IsAssignableFrom(x))
                .Singleton();
        }
    }
}