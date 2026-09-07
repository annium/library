using System;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Connectors.Market;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Core.Internal.Shared.Pooling;

namespace Annium.Finance.Providers.Core.Internal.Market;

/// <summary>
/// Default <see cref="IPooledMarketConnectorFactory"/> implementation. Keeps one connector per distinct
/// settings value, built through <paramref name="factory"/> on the first lease and disposed once the last
/// lease is returned.
/// </summary>
/// <param name="factory">The factory the pool builds shared connectors with.</param>
internal sealed class PooledMarketConnectorFactory(IMarketConnectorFactory factory)
    : IPooledMarketConnectorFactory,
        IAsyncDisposable
{
    /// <summary>The connectors handed out as leases, by settings.</summary>
    private readonly ConnectorPool<MarketSettings, IMarketConnector> _pool = new();

    /// <summary>
    /// Takes a lease on the connector shared by everything using these settings, building it on the first
    /// lease.
    /// </summary>
    /// <param name="settings">The market settings identifying the provider and market to connect to.</param>
    /// <returns>A lease on the shared market connector.</returns>
    public IMarketConnector Create(MarketSettings settings)
    {
        var (connector, release) = _pool.Acquire(settings, () => factory.Create(settings));

        return new PooledMarketConnector(connector, release);
    }

    /// <summary>
    /// Disposes every connector still held by the pool.
    /// </summary>
    /// <returns>A task that completes once they are all torn down.</returns>
    public async ValueTask DisposeAsync() => await _pool.DisposeAsync();
}
