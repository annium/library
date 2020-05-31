using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using XRest.Sources.Components;
using XRest.Sources.Components.Internal;

namespace XRest.Sources
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Api.ServicePack>();
            Add<Assembly.ServicePack>();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<ILoader, Loader>();
        }
    }
}