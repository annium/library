using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using XRest.Sources.Api.Components;
using XRest.Sources.Api.Components.Internal;

namespace XRest.Sources.Api
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddHttpRequestFactory();
            services.AddSingleton<ILoader, Loader>();
        }
    }
}