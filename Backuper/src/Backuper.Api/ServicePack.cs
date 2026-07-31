using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Yaml;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime;
using Annium.Extensions.Jobs;
using Annium.Logging.Console;
using Annium.Logging.Shared;
using Backuper.Api.Config;
using Backuper.Api.State;
using Backuper.Api.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Api;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Connection.Abstract.ServicePack>();
        Add<Connection.PostgreSQL.ServicePack>();
        Add<Notification.Abstract.ServicePack>();
        Add<Notification.Slack.ServicePack>();
        Add<Storage.ServicePack>();
    }

    public override async Task RegisterAsync(
        IServiceContainer container,
        IServiceProvider provider,
        CancellationToken ct
    )
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        await container.AddConfigurationAsync<Configuration>(
            x => x.AddYamlFile(Path.Combine("configuration", "config.yml")),
            ct
        );
        // container.AddFileSystemStorage().AddS3Storage();

        container.Add<StateFactory>().AsSelf().Singleton();
        container.Add<StateManager>().AsSelf().Singleton();
        container.Add<Func<State.State>>(sp => () => sp.GetRequiredService<StateManager>().State!).Singleton();
        container.Add<Namer>().AsSelf().Singleton();

        container.AddScheduler();
        container.AddMediator();
        container.AddLogging();
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        provider.UseLogging(route => route.UseConsole());

        var stateFactory = provider.GetRequiredService<StateFactory>();
        var stateManager = provider.GetRequiredService<StateManager>();

        try
        {
            var state = stateFactory.GetState();
            stateManager.SetState(state);
        }
        catch (AggregateException ex)
        {
            throw ex.InnerException!;
        }

        return Task.CompletedTask;
    }
}
