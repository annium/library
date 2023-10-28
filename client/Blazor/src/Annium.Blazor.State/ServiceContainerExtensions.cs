using Annium.Blazor.State;
using Annium.Blazor.State.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddStates(this IServiceContainer container)
    {
        container.Add<ILocalStorage, LocalStorage>().Singleton();
        container.Add<ISessionStorage, SessionStorage>().Singleton();

        container
            .AddAll()
            .AssignableTo<StateBase>()
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .AsSelf()
            .AsInterfaces()
            .Scoped();

        return container;
    }
}
