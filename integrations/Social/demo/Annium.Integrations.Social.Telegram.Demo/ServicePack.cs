using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Yaml;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Integrations.Social.Telegram.Demo.Internal;
using Annium.Logging.Console;
using Annium.Logging.Shared;

namespace Annium.Integrations.Social.Telegram.Demo;

internal class ServicePack : ServicePackBase
{
    public override async Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        container.AddRuntime(GetType().Assembly);
        await container.AddConfigurationAsync<Dictionary<string, TelegramBotConfiguration>>(
            cfg => cfg.AddYamlFile(Path.Combine("configuration", "bots.yml")),
            ct
        );
    }

    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddTime().WithRealTime().SetDefault();
        container.AddTelegramBot(
            "demo",
            sp => sp.Resolve<Dictionary<string, TelegramBotConfiguration>>()["demo"],
            opts =>
            {
                // opts.UseWebhookReceiver();
                opts.UsePollingReceiver();
                opts.UseHandler<EchoTelegramMessageHandler>();
            }
        );
        container.AddMapper();
        container.AddLogging();

        return Task.CompletedTask;
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        provider.UseLogging(route => route.UseConsole());

        return Task.CompletedTask;
    }
}
