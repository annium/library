using Annium.Blazor.State.Internal;
using Annium.Components.State.Core;
using Annium.Core.DependencyInjection;

namespace Annium.Blazor.State;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddStates(this IServiceContainer container)
    {
        container.Add<ILocalStorage, LocalStorage>().Singleton();
        container.Add<ISessionStorage, SessionStorage>().Singleton();

        container.AddAll()
            .AssignableTo<StateBase>()
            .Where(x => x.IsClass)
            .AsSelf()
            .AsInterfaces()
            .Scoped();

        return container;
    }
}