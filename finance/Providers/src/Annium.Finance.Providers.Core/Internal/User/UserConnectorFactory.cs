using System;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Core.User;
using Annium.Logging;

namespace Annium.Finance.Providers.Core.Internal.User;

/// <summary>
/// Default <see cref="IUserConnectorFactory"/> implementation. Builds a standalone user connector with a
/// status monitor of its own, through the provider-specific instance factory registered for the settings'
/// provider key.
/// </summary>
/// <remarks>
/// This used to open a DI scope per connector, for one reason: a fresh <see cref="StatusMonitor"/>, since
/// every other service in the construction path is either stateless or shared across the process. The scope
/// silently made those shared things per-connector too - a rate limiter each, a server time sync loop each -
/// which is what a provider's limits are not counted by. The monitor is built here instead, and nothing in
/// the path needs a scope, so the factory builds in whatever provider it was resolved through.
/// </remarks>
/// <param name="sp">The service provider the connector's dependencies are resolved from.</param>
/// <param name="logger">The logger instance.</param>
internal class UserConnectorFactory(IServiceProvider sp, ILogger logger) : IUserConnectorFactory, ILogSubject
{
    /// <summary>Gets the logger instance.</summary>
    public ILogger Logger { get; } = logger;

    /// <summary>
    /// Creates a user connector configured with the given settings.
    /// </summary>
    /// <param name="settings">The user settings identifying the provider account to connect to.</param>
    /// <returns>A new user connector instance the caller owns.</returns>
    public IUserConnector Create(UserSettings settings)
    {
        var providerKey = settings.GetProviderKey();

        this.Trace("{key} - create disposable box for {settings}", providerKey, settings);
        var disposable = Disposable.AsyncBox(Logger);

        this.Trace("{key} - create status monitor for {settings}", providerKey, settings);
        var monitor = new StatusMonitor(Logger);

        this.Trace("{key} - resolve factory for {settings}", providerKey, settings);
        var factory = sp.ResolveKeyed<IUserConnectorInstanceFactory>(settings.Provider);

        this.Trace<ProviderKey, UserSettings, string>(
            "{key} - create connector for {settings} with {factory}",
            providerKey,
            settings,
            factory.GetFullId()
        );

        return factory.Create(settings, monitor, disposable);
    }
}
