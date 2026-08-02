using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.XRest.Clients.Shared.Components;
using Annium.XRest.Clients.Shared.Internal.Components;

namespace Annium.XRest.Clients.Shared;

public class ServicePack : ServicePackBase
{
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.Add<IApiModelLoader, ApiModelLoader>().Singleton();
        container.Add<ITemplateWriter, TemplateWriter>().Singleton();
        container.AddResourceLoader();

        return Task.CompletedTask;
    }
}
