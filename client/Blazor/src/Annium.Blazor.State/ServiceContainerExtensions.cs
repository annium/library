using Annium.Blazor.State.Internal;
using Annium.Core.DependencyInjection;

namespace Annium.Blazor.State;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddStorages(this IServiceContainer container)
    {
        container.Add<ILocalStorage, LocalStorage>().Singleton();
        container.Add<ISessionStorage, SessionStorage>().Singleton();

        container.AddAll()
            .AssignableTo<IStore>()
            .Where(x => x.IsClass)
            .AsInterfaces()
            .Scoped();

        return container;
    }
}