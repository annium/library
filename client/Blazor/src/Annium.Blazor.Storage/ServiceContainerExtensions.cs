using System.Reflection;
using Annium.Blazor.Storage;
using Annium.Blazor.Storage.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddStorages(this IServiceContainer container)
        {
            container.Add<ILocalStorage, LocalStorage>().Singleton();
            container.Add<ISessionStorage, SessionStorage>().Singleton();

            container.AddAll(Assembly.GetCallingAssembly(), false)
                .AssignableTo<IStore>()
                .Where(x => x.IsClass)
                .AsInterfaces()
                .Singleton();

            return container;
        }
    }
}