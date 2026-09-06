using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Tables;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Abstractions.Domain.User.Operations;
using Annium.Finance.Providers.Abstractions.Domain.User.Requests;

namespace Annium.Finance.Providers.Core.Internal.User;

/// <summary>
/// A lease on a shared user connector: disposing it gives the lease back rather than tearing the
/// connector down, which happens once the last holder lets go.
/// </summary>
/// <param name="inner">The shared connector.</param>
/// <param name="release">Gives this lease back to the pool.</param>
internal sealed class PooledUserConnector(IUserConnector inner, Func<ValueTask> release) : IUserConnector
{
    /// <summary>Set on the first disposal, so a second one is a no-op rather than a second release.</summary>
    private int _isReleased;

    /// <summary>Gets the current connection status of the connector.</summary>
    public ConnectorStatus Status => inner.Status;

    /// <summary>An observable stream of asset balance changes.</summary>
    public IObservable<ChangeEvent<AssetModel>> Assets => inner.Assets;

    /// <summary>An observable stream of position changes.</summary>
    public IObservable<ChangeEvent<PositionModel>> Positions => inner.Positions;

    /// <summary>An observable stream of order changes.</summary>
    public IObservable<ChangeEvent<OrderModel>> Orders => inner.Orders;

    /// <summary>An observable stream of executed trades.</summary>
    public IObservable<TradeModel> Trades => inner.Trades;

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

    /// <summary>Raised during a sync cycle, before the connector resumes real-time updates.</summary>
    public event Func<UserSettings, IUserProvider, Task> OnSync
    {
        add => inner.OnSync += value;
        remove => inner.OnSync -= value;
    }

    /// <summary>Forces a resync.</summary>
    public void Sync() => inner.Sync();

    /// <summary>Sets the leverage on a position.</summary>
    /// <param name="position">The position to set leverage on.</param>
    /// <param name="leverage">The leverage to set.</param>
    /// <returns>The outcome of the operation.</returns>
    public Task<UserResult> SetLeverageAsync(PositionModel position, decimal leverage) =>
        inner.SetLeverageAsync(position, leverage);

    /// <summary>Places an order.</summary>
    /// <param name="request">The order to place.</param>
    /// <returns>The placed order, or the failure that prevented it.</returns>
    public Task<UserResult<OrderModel?>> InitOrderAsync(IInitOrderRequest request) => inner.InitOrderAsync(request);

    /// <summary>Modifies an existing order.</summary>
    /// <param name="request">The modification to apply.</param>
    /// <returns>The modified order, or the failure that prevented it.</returns>
    public Task<UserResult<OrderModel?>> ModifyOrderAsync(IModifyOrderRequest request) =>
        inner.ModifyOrderAsync(request);

    /// <summary>Cancels an order.</summary>
    /// <param name="request">The order to cancel.</param>
    /// <returns>The outcome of the operation.</returns>
    public Task<UserResult> CancelOrderAsync(ICancelOrderRequest request) => inner.CancelOrderAsync(request);

    /// <summary>Cancels every open order on a symbol.</summary>
    /// <param name="symbol">The symbol to cancel orders on.</param>
    /// <returns>The outcome of the operation.</returns>
    public Task<UserResult> CancelAllOrdersAsync(string symbol) => inner.CancelAllOrdersAsync(symbol);

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
