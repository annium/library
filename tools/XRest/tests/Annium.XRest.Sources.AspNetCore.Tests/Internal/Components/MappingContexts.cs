using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging;
using Annium.Net.Types;
using Annium.XRest.Sources.AspNetCore.Internal.Components;

namespace Annium.XRest.Sources.AspNetCore.Tests.Internal.Components;

internal static class MappingContexts
{
    /// <summary>
    /// Builds a context around the real model mapper — the builders map every parameter, body and
    /// response type through it, so a fake would only pin the test's own idea of mapping.
    /// </summary>
    /// <returns>A context backed by a live <see cref="IModelMapper"/>.</returns>
    public static MappingContext Create()
    {
        var container = new ServiceContainer();
        container.AddRuntime(typeof(MappingContexts).Assembly);
        // the mapper's processors take an ILogger; the test project has no logging package, and
        // nothing here logs anything worth reading
        container.Add<ILogger>(VoidLogger.Instance).AsSelf().Singleton();
        container.AddModelMapper();
        var provider = container.BuildServiceProvider();

        return new MappingContext(provider.Resolve<IModelMapper>(), provider.Resolve<IMapperConfig>());
    }
}
