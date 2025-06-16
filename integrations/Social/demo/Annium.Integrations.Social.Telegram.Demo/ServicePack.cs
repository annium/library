using System;
using System.Collections.Generic;
using System.IO;
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
    public override void Configure(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddConfiguration<Dictionary<string, TelegramBotConfiguration>>(cfg =>
            cfg.AddYamlFile(Path.Combine("configuration", "bots.yml"))
        );
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
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
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}
