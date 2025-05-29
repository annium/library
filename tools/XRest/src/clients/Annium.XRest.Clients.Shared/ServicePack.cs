using System;
using Annium.Core.DependencyInjection;
using Annium.XRest.Clients.Shared.Components;
using Annium.XRest.Clients.Shared.Internal.Components;

namespace Annium.XRest.Clients.Shared;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<IApiModelLoader, ApiModelLoader>().Singleton();
        container.Add<ITemplateWriter, TemplateWriter>().Singleton();
        container.AddResourceLoader();
    }
}
