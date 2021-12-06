using System;
using Annium.Core.DependencyInjection;
using XRest.Sources.Assembly.Components;
using XRest.Sources.Assembly.Components.Internal;

namespace XRest.Sources.Assembly;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<ILoader, Loader>().Singleton();
        container.Add<IParser, Parser>().Singleton();
    }
}