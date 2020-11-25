using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;
using XRest.Core.Components;
using XRest.Core.Components.Implementations;

namespace XRest.Core
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddRuntimeTools(GetType().Assembly, true);
            container.AddTimeProvider();
            container.AddJsonSerializers((sp, opts) => opts.ConfigureDefault(sp.Resolve<ITypeManager>()));

            container.Add<ITemplateWriter, TemplateWriter>().Singleton();

            container.AddAssemblyLoader();
            container.AddResourceLoader();
        }
    }
}