using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using XRest.Core.Components;
using XRest.Core.Components.Implementations;
using XRest.Core.Views.Profiles;

namespace XRest.Core
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);

            services.AddSingleton<IAssemblyLoader, AssemblyLoader>();
            services.AddSingleton<ITemplateWriter, TemplateWriter>();

            Mapper.AddProfile(new HttpMethodProfile());
            services.AddResourceLoader();
            services.AddLoadContextFactories();
        }
    }
}