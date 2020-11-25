using System;
using Annium.Core.DependencyInjection;
using XRest.Sources.Api.Components;
using XRest.Sources.Api.Components.Internal;

namespace XRest.Sources.Api
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddHttpRequestFactory();
            container.Add<ILoader, Loader>().Singleton();
        }
    }
}