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

/// <summary>
/// Wires the demo bot: runtime scanning, bot configuration from bots.yml, and a polling receiver with an echo handler.
/// </summary>
internal class ServicePack : ServicePackBase
{
    /// <summary>
    /// Registers runtime type scanning and loads the bot configurations: the non-secret settings from
    /// bots.yml, the secrets from the environment on top of them.
    /// </summary>
    /// <param name="container">The container to configure.</param>
    /// <param name="ct">The token that cancels configuration.</param>
    /// <returns>A task that completes once configuration is done.</returns>
    public override async Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        container.AddRuntime(GetType().Assembly);
        await container.AddConfigurationAsync<Dictionary<string, TelegramBotConfiguration>>(
            cfg => cfg.AddYamlFile(Path.Combine("configuration", "bots.yml")).Add(GetSecrets()),
            ct
        );
    }

    /// <summary>
    /// Reads the demo bot's secrets from the environment. Sources registered later win, so this layer
    /// fills in what bots.yml deliberately leaves out.
    /// </summary>
    /// <returns>The secret slice of the configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the bot token is not in the environment.</exception>
    private static object GetSecrets()
    {
        var token =
            Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")
            ?? throw new InvalidOperationException(
                "TELEGRAM_BOT_TOKEN is not set. Get a token from BotFather and export it before running the demo."
            );

        return new Dictionary<string, object>
        {
            ["demo"] = new Dictionary<string, object>
            {
                ["token"] = token,
                ["webhook"] = new Dictionary<string, object>
                {
                    // only meaningful for UseWebhookReceiver; harmless empty defaults under polling
                    ["secret_token"] =
                        Environment.GetEnvironmentVariable("TELEGRAM_WEBHOOK_SECRET_TOKEN") ?? string.Empty,
                    ["external_address"] =
                        Environment.GetEnvironmentVariable("TELEGRAM_WEBHOOK_ADDRESS") ?? "https://localhost:18080",
                },
            },
        };
    }

    /// <summary>
    /// Registers the demo bot with a polling receiver and the echo handler, plus time, mapper and logging.
    /// </summary>
    /// <param name="container">The container to register into.</param>
    /// <param name="provider">The provider available for resolving dependencies during registration.</param>
    /// <param name="ct">The token that cancels registration.</param>
    /// <returns>A task that completes once registration is done.</returns>
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

    /// <summary>
    /// Routes logging to the console.
    /// </summary>
    /// <param name="provider">The provider to resolve services from.</param>
    /// <param name="ct">The token that cancels setup.</param>
    /// <returns>A task that completes once setup is done.</returns>
    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        provider.UseLogging(route => route.UseConsole());

        return Task.CompletedTask;
    }
}
