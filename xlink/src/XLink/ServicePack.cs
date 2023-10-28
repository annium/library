using System;
using Annium.Core.DependencyInjection;

namespace XLink;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // register and setup services
    }

    public override void Setup(IServiceProvider provider)
    {
        // setup post-configured services
    }
}
