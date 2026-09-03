using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Extensions.Arguments;
using Annium.Logging.Console;
using Annium.Logging.Shared;
using Annium.Versioning.Services;

namespace Annium.Versioning;

internal class ServicePack : ServicePackBase
{
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddArguments();
        container.AddLogging();
        container.AddMapper();

        container.Add<IGitTagService, GitTagService>().Singleton();
        container.Add<IVersionService, VersionService>().Singleton();

        return Task.CompletedTask;
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        provider.UseLogging(route => route.UseConsole());

        return Task.CompletedTask;
    }
}
