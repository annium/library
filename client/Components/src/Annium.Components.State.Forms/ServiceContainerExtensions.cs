using Annium.Components.State.Forms;
using Annium.Components.State.Forms.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddComponentFormStateFactory(
            this IServiceContainer container
        )
        {
            container.Add<IStateFactory, StateFactory>().Singleton();

            return container;
        }
    }
}