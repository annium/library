using System;
using Annium.Core.DependencyInjection;
using XRest.Core;
using XRest.Source.Components;
using XRest.Source.Internal.Components;

namespace XRest.Source;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddHttpRequestFactory().SetDefault();
        container.AddXRestSerializer();
        container.Add<ILoader, Loader>().Singleton();
    }
}