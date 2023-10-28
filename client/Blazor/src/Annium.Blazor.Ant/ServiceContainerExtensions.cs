using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddAntDesign(this IServiceContainer container)
    {
        container.Collection.AddAntDesign();

        return container;
    }
}
