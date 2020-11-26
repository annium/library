using System;
using Annium.Core.DependencyInjection;
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
            container.AddJsonSerializers();

            container.Add<ITemplateWriter, TemplateWriter>().Singleton();

            container.AddAssemblyLoader();
            container.AddResourceLoader();
        }
    }
}