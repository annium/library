using System.Runtime.CompilerServices;
using Annium.Blazor.Routing.Internal;
using Annium.Blazor.Routing.Internal.Implementations;
using Annium.Core.DependencyInjection;

[assembly: InternalsVisibleTo("Annium.Blazor.Routing.Tests")]

namespace Annium.Blazor.Routing
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddRouting(this IServiceContainer container)
        {
            container.Add<RouteManager>().AsInterfaces().Singleton();
            container.Add<RouteFactory>().AsInterfaces().Singleton();
            container.AddAll().AssignableTo<IRouting>().AsInterfaces().Singleton();

            return container;
        }
    }
}