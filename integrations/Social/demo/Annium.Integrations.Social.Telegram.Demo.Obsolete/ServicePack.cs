using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.Integrations.Social.Telegram.Demo.Obsolete;

/// <summary>
/// Empty service pack of the legacy Telegram demo host, kept as the wiring skeleton for the obsolete integration.
/// </summary>
internal class ServicePack : ServicePackBase
{
    /// <summary>
    /// Registers nothing; the legacy demo has no configuration of its own.
    /// </summary>
    /// <param name="container">The container to configure.</param>
    /// <param name="ct">The token that cancels configuration.</param>
    /// <returns>A task that completes once configuration is done.</returns>
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        // register configurations
        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers nothing; the legacy demo has no services of its own.
    /// </summary>
    /// <param name="container">The container to register into.</param>
    /// <param name="provider">The provider available for resolving dependencies during registration.</param>
    /// <param name="ct">The token that cancels registration.</param>
    /// <returns>A task that completes once registration is done.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        // register and setup services
        return Task.CompletedTask;
    }

    /// <summary>
    /// Performs no setup; the legacy demo has no services to initialize.
    /// </summary>
    /// <param name="provider">The provider to resolve services from.</param>
    /// <param name="ct">The token that cancels setup.</param>
    /// <returns>A task that completes once setup is done.</returns>
    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        // setup post-configured services
        return Task.CompletedTask;
    }
}
