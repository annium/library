using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Finance.Providers.Core.Internal.Shared.Pooling;

/// <summary>
/// Keeps one connector per key alive for as long as anyone holds a lease on it.
/// </summary>
/// <remarks>
/// A provider charges its rate limit per account, not per connector, so components that each want a
/// connector for the same settings - one per instrument, say - must not each get their own. Leases are
/// counted rather than tracked by identity: the pool only needs to know when the last one is gone.
/// </remarks>
/// <typeparam name="TKey">The settings a connector is built for; used as the sharing key.</typeparam>
/// <typeparam name="TConnector">The connector type.</typeparam>
internal sealed class ConnectorPool<TKey, TConnector> : IAsyncDisposable
    where TKey : notnull
    where TConnector : class, IAsyncDisposable
{
    /// <summary>Guards <see cref="_entries"/> and every count that lives in it.</summary>
    private readonly Lock _lock = new();

    /// <summary>The live connectors, by the settings they were built for.</summary>
    private readonly Dictionary<TKey, Entry> _entries = new();

    /// <summary>Set once the pool is disposed; no lease is handed out after that.</summary>
    private bool _isDisposed;

    /// <summary>
    /// Takes a lease on the connector for <paramref name="key"/>, building it if this is the first one.
    /// </summary>
    /// <remarks>
    /// <paramref name="create"/> runs under the lock. It only resolves and wires services, and holding the
    /// lock across it is what makes two simultaneous first callers share one connector instead of racing
    /// to build two and leaking the loser.
    /// </remarks>
    /// <param name="key">The settings to share by.</param>
    /// <param name="create">Builds the connector when none is live for this key.</param>
    /// <returns>The shared connector, and the release to call when the caller is done with it.</returns>
    /// <exception cref="ObjectDisposedException">The pool has been disposed.</exception>
    public (TConnector Connector, Func<ValueTask> Release) Acquire(TKey key, Func<TConnector> create)
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            if (!_entries.TryGetValue(key, out var entry))
            {
                entry = new Entry(create());
                _entries[key] = entry;
            }

            entry.Count++;

            return (entry.Connector, () => ReleaseAsync(key));
        }
    }

    /// <summary>
    /// Disposes every connector still pooled.
    /// </summary>
    /// <returns>A task that completes once they are all torn down.</returns>
    public async ValueTask DisposeAsync()
    {
        List<TConnector> connectors;

        lock (_lock)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            connectors = [.. _entries.Values.Select(x => x.Connector)];
            _entries.Clear();
        }

        foreach (var connector in connectors)
            await connector.DisposeAsync();
    }

    /// <summary>
    /// Gives up one lease, tearing the connector down when it was the last.
    /// </summary>
    /// <param name="key">The settings the lease was taken for.</param>
    /// <returns>A task that completes once the connector is torn down, or immediately if others hold it.</returns>
    private async ValueTask ReleaseAsync(TKey key)
    {
        TConnector connector;

        lock (_lock)
        {
            if (!_entries.TryGetValue(key, out var entry))
                return;

            if (--entry.Count > 0)
                return;

            _entries.Remove(key);
            connector = entry.Connector;
        }

        // outside the lock: tearing a connector down drains its executor, and nothing about that needs
        // to block another key's lease
        await connector.DisposeAsync();
    }

    /// <summary>
    /// A live connector and the number of leases held on it.
    /// </summary>
    /// <param name="Connector">The connector.</param>
    private sealed record Entry(TConnector Connector)
    {
        /// <summary>Gets or sets how many leases are outstanding.</summary>
        public int Count { get; set; }
    }
}
