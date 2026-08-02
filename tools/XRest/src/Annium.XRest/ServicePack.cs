using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Extensions.Arguments;
using Annium.Logging.Console;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.XRest.Core;
using Constants = Annium.XRest.Core.Constants;

namespace Annium.XRest;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Clients.Csharp.ServicePack>();
        Add<Clients.Shared.ServicePack>();
    }

    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddArguments();
        container.AddLogging();
        container.AddMapper();
        container.AddHttpRequestFactory(Constants.IndexKey);
        container.AddXRestSerializer();

        return Task.CompletedTask;
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        provider.UseLogging(route => route.UseConsole());

        return Task.CompletedTask;
    }
}
