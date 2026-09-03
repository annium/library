using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Annium.Social.Telegram.Integration.Receivers;
using Annium.Social.Telegram.Internal;
using Annium.Social.Telegram.Internal.Integration;
using Annium.Social.Telegram.Internal.Integration.Receivers;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Social.Telegram.Tests;

/// <summary>
/// Tests for what <c>AddTelegramBot</c> wires up: which transport a bot ends up with, and the lifetime the
/// receiver is registered under. One bot per scenario is registered up front, since the fixture closes
/// registrations once it starts.
/// </summary>
public class RegistrationTests : TestBase
{
    /// <summary>
    /// A port that was free a moment ago and has nothing listening on it now.
    /// </summary>
    private static readonly ushort _deadPort = ReserveDeadPort();

    /// <summary>
    /// Registers one bot per scenario: polling by configuration, webhook by configuration, and a webhook
    /// configuration overridden back to polling.
    /// </summary>
    /// <param name="outputHelper">The xunit output helper test logs are written to.</param>
    public RegistrationTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddTelegramBot("polling", _ => new TelegramBotConfiguration { Token = "test-token" }, _ => { });
            container.AddTelegramBot("webhook", _ => WebhookConfiguration(), _ => { });
            container.AddTelegramBot("overridden", _ => WebhookConfiguration(), opts => opts.UsePollingReceiver());

            // the webhook receiver calls setWebhook the moment it is constructed. Left alone it would build
            // its context from the token and reach the real api.telegram.org, so both webhook bots are
            // pointed at a port nothing listens on: the call is refused locally and the receiver — whose
            // type is all these tests read — is still the one under test
            container.Add(LocalApiContext).AsKeyed<ApiContext>("webhook").Singleton();
            container.Add(LocalApiContext).AsKeyed<ApiContext>("overridden").Singleton();
        });
    }

    /// <summary>
    /// A bot with no webhook configured long-polls. Picking the wrong transport is silent — the bot simply
    /// never receives anything — so the choice is worth pinning in both directions.
    /// </summary>
    [Fact]
    public void AddTelegramBot_NoWebhookConfigured_UsesPolling()
    {
        // act
        var receiver = Provider.ResolveKeyed<ITelegramMessageReceiver>("polling");

        // assert
        receiver.As<PollingMessageReceiver>();
    }

    /// <summary>
    /// A bot with a webhook configured uses the webhook receiver instead.
    /// </summary>
    [Fact]
    public void AddTelegramBot_WebhookConfigured_UsesWebhook()
    {
        // act
        var receiver = Provider.ResolveKeyed<ITelegramMessageReceiver>("webhook");

        // assert
        receiver.As<WebhookMessageReceiver>();
    }

    /// <summary>
    /// An explicit <c>UsePollingReceiver</c> wins over the webhook the configuration would otherwise select.
    /// </summary>
    [Fact]
    public void AddTelegramBot_UsePollingReceiver_OverridesTheConfiguredWebhook()
    {
        // act
        var receiver = Provider.ResolveKeyed<ITelegramMessageReceiver>("overridden");

        // assert
        receiver.As<PollingMessageReceiver>();
    }

    /// <summary>
    /// The receiver is a singleton. It owns a background loop and is <c>IAsyncDisposable</c>, while its only
    /// consumer — the bot host — is a root singleton: a scoped registration made every receiver a captive
    /// dependency of that host, which is the bug this pins.
    /// </summary>
    [Fact]
    public void AddTelegramBot_ReceiverResolvedTwice_IsTheSameInstance()
    {
        // act - two separate scopes: resolving twice from the root cannot tell a singleton from a scoped
        // registration, which is exactly the mistake this pins
        using var left = Provider.CreateScope();
        using var right = Provider.CreateScope();
        var first = left.ServiceProvider.ResolveKeyed<ITelegramMessageReceiver>("polling");
        var second = right.ServiceProvider.ResolveKeyed<ITelegramMessageReceiver>("polling");

        // assert
        ReferenceEquals(first, second).IsTrue("the receiver registration is a singleton");
    }

    /// <summary>
    /// Builds an API context aimed at a local port nothing listens on, so no request leaves the machine.
    /// </summary>
    /// <param name="sp">The provider to resolve the HTTP factory and serializer from.</param>
    /// <param name="key">The key of the bot the context belongs to.</param>
    /// <returns>The API context.</returns>
    private static ApiContext LocalApiContext(IServiceProvider sp, object key) =>
        new(
            new Uri($"http://127.0.0.1:{_deadPort}/bottest-token"),
            sp.ResolveKeyed<IHttpRequestFactory>(Internal.Constants.ServiceKey),
            sp.ResolveKeyed<ISerializer<Stream>>(Internal.Constants.SerializerKey)
        );

    /// <summary>
    /// Builds a configuration that selects the webhook transport. The receiver calls setWebhook and starts a
    /// server on construction; the address is unroutable and the port is left to the OS, since these tests
    /// only inspect which type was chosen.
    /// </summary>
    /// <returns>The bot configuration.</returns>
    private static ushort ReserveDeadPort()
    {
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = (ushort)((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();

        return port;
    }

    /// <summary>
    /// Builds a configuration selecting the webhook transport.
    /// </summary>
    /// <returns>The bot configuration.</returns>
    private static TelegramBotConfiguration WebhookConfiguration() =>
        new()
        {
            Token = "test-token",
            Webhook = new TelegramBotWebhookConfiguration
            {
                InternalPort = 0,
                ExternalAddress = new Uri("https://example.test/hook"),
                SecretToken = "s3cret",
            },
        };
}
