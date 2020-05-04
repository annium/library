using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xmg.Core.Tools;
using Xmg.Core.Tools.Implementations;

namespace Xmg.Core
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // tools
            services.AddSingleton<ITemplateWriter, TemplateWriter>();

            // externals
            services.AddResourceLoader();
        }
    }
}