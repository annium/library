using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using XRest.Core.Components;
using XRest.Core.Components.Implementations;

namespace XRest.Core
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);

            services.AddSingleton<IAssemblyLoader, AssemblyLoader>();
            services.AddSingleton<ITemplateWriter, TemplateWriter>();
        }
    }
}