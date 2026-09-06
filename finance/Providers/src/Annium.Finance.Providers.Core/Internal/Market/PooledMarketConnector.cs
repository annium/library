using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Connectors.Market;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Domain.Market;

namespace Annium.Finance.Providers.Core.Internal.Market;

/// <summary>
/// A lease on a shared market connector: disposing it gives the lease back rather than tearing the
/// connector down, which happens once the last holder lets go.
/// </summary>
/// <param name="inner">The shared connector.</param>
/// <param name="release">Gives this lease back to the pool.</param>
internal sealed class PooledMarketConnector(IMarketConnector inner, Func<ValueTask> release) : IMarketConnector
{
    /// <summary>Set on the first disposal, so a second one is a no-op rather than a second release.</summary>
    private int _isReleased;

    /// <summary>Gets the current connection status of the connector.</summary>
    public ConnectorStatus Status => inner.Status;

    /// <summary>Gets the resources (assets) currently known to the connector, as loaded on the last sync.</summary>
    public IReadOnlyCollection<ResourceModel> Resources => inner.Resources;

    /// <summary>Gets the instruments currently known to the connector, as loaded on the last sync.</summary>
    public IReadOnlyCollection<InstrumentModel> Instruments => inner.Instruments;

    /// <summary>An observable stream of instrument ticker updates.</summary>
    public IObservable<InstrumentTicker> Tickers => inner.Tickers;

    /// <summary>Raised whenever <see cref="Status"/> changes, with the new status.</summary>
    public event Action<ConnectorStatus> OnStatusChanged
    {
        add => inner.OnStatusChanged += value;
        remove => inner.OnStatusChanged -= value;
    }

    /// <summary>Raised when the connector encounters an error.</summary>
    public event Action<ConnectorError> OnError
    {
        add => inner.OnError += value;
        remove => inner.OnError -= value;
    }

    /// <summary>Raised during a sync cycle, once resources and instruments have been reloaded.</summary>
    public event Func<
        MarketSettings,
        IReadOnlyCollection<ResourceModel>,
        IReadOnlyCollection<InstrumentModel>,
        Task
    > OnSync
    {
        add => inner.OnSync += value;
        remove => inner.OnSync -= value;
    }

    /// <summary>Forces a resync.</summary>
    public void Sync() => inner.Sync();

    /// <summary>Subscribes to ticker updates for the given instrument symbols.</summary>
    /// <param name="symbols">The instrument symbols to subscribe to.</param>
    public void SubscribeTickers(IReadOnlyCollection<string> symbols) => inner.SubscribeTickers(symbols);

    /// <summary>Unsubscribes from ticker updates for the given instrument symbols.</summary>
    /// <param name="symbols">The instrument symbols to unsubscribe from.</param>
    public void UnsubscribeTickers(IReadOnlyCollection<string> symbols) => inner.UnsubscribeTickers(symbols);

    /// <summary>
    /// Gives the lease back. The shared connector survives until the last lease is returned.
    /// </summary>
    /// <returns>A task that completes once the release is recorded.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _isReleased, 1) != 0)
            return;

        await release();
    }
}
