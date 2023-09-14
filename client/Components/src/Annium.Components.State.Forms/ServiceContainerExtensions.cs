using Annium.Components.State.Forms;
using Annium.Components.State.Forms.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddStateFactory(
        this IServiceContainer container
    )
    {
        container.Add<IStateFactory, StateFactory>().Singleton();

        return container;
    }
}