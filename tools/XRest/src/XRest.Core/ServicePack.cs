using System;
using Annium.Core.DependencyInjection;
using XRest.Core.Components;
using XRest.Core.Internal.Components;

namespace XRest.Core;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<ITemplateWriter, TemplateWriter>().Singleton();
        container.AddResourceLoader();
    }
}