using System;
using Annium.Core.DependencyInjection;
using XRest.Clients.Shared.Components;
using XRest.Clients.Shared.Internal.Components;

namespace XRest.Clients.Shared;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<IApiModelLoader, ApiModelLoader>().Singleton();
        container.Add<ITemplateWriter, TemplateWriter>().Singleton();
        container.AddResourceLoader();
    }
}
