using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.XRest.Demo.Client;

internal class ServicePack : ServicePackBase
{
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        // register configurations

        return Task.CompletedTask;
    }

    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        // register and setup services

        return Task.CompletedTask;
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        // setup post-configured services

        return Task.CompletedTask;
    }
}
