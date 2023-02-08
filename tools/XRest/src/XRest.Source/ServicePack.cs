using System;
using Annium.Core.DependencyInjection;
using XRest.Source.Components;
using XRest.Source.Internal.Components;

namespace XRest.Source;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<ILoader, Loader>().Singleton();
    }
}