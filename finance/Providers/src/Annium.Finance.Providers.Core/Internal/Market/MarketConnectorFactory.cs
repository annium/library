using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Connectors.Market;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Core.Internal.Shared.Pooling;
using Annium.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Finance.Providers.Core.Internal.Market;

/// <summary>
/// Default <see cref="IMarketConnectorFactory"/> implementation. Builds a standalone market connector in its own
/// DI scope, resolving the provider-specific instance factory registered for the settings' provider key.
/// </summary>
/// <param name="sp">The root service provider used to create the connector's own DI scope.</param>
/// <param name="logger">The logger instance.</param>
internal class MarketConnectorFactory(IServiceProvider sp, ILogger logger)
    : IMarketConnectorFactory,
        ILogSubject,
        IAsyncDisposable
{
    /// <summary>Gets the logger instance.</summary>
    public ILogger Logger { get; } = logger;

    /// <summary>The connectors shared through <see cref="CreatePooled" />, by settings.</summary>
    private readonly ConnectorPool<MarketSettings, IMarketConnector> _pool = new();

    /// <summary>
    /// Takes a lease on the connector shared by everything using these settings, building it on the
    /// first lease.
    /// </summary>
    /// <param name="settings">The market settings identifying the provider and market to connect to.</param>
    /// <returns>A lease on the shared market connector.</returns>
    public IMarketConnector CreatePooled(MarketSettings settings)
    {
        var (connector, release) = _pool.Acquire(settings, () => Create(settings));

        return new PooledMarketConnector(connector, release);
    }

    /// <summary>
    /// Disposes every connector still held by the pool.
    /// </summary>
    /// <returns>A task that completes once they are all torn down.</returns>
    public async ValueTask DisposeAsync() => await _pool.DisposeAsync();

    /// <summary>
    /// Creates a market connector configured with the given settings.
    /// </summary>
    /// <param name="settings">The market settings identifying the provider and market to connect to.</param>
    /// <returns>A new market connector instance the caller owns.</returns>
    public IMarketConnector Create(MarketSettings settings)
    {
        var providerKey = settings.GetProviderKey();

        this.Trace("{key} - create disposable box for {settings}", providerKey, settings);
        var disposable = Disposable.AsyncBox(Logger);

        this.Trace("{key} - create new scope for {settings}", providerKey, settings);
        var scope = sp.CreateAsyncScope();

        this.Trace("{key} - resolve factory for {settings}", providerKey, settings);
        var factory = scope.ServiceProvider.ResolveKeyed<IMarketConnectorInstanceFactory>(settings.Provider);

        this.Trace<ProviderKey, MarketSettings, string>(
            "{key} - create connector for {settings} with {factory}",
            providerKey,
            settings,
            factory.GetFullId()
        );
        var connector = factory.Create(settings, disposable);

        // the scope is not put in the connector's own box: that box drains its asynchronous entries
        // concurrently, so the scope would tear down alongside the executor rather than after it, and
        // the executor may still be draining a sync cycle that is using what the scope owns
        return new ScopedMarketConnector(connector, scope);
    }
}
