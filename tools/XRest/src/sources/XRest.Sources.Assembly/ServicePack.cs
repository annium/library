using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using XRest.Sources.Assembly.Components;
using XRest.Sources.Assembly.Components.Internal;

namespace XRest.Sources.Assembly
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<ILoader, Loader>();
            services.AddSingleton<IParser, Parser>();
        }
    }
}