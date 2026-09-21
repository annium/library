using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Core.Shared.TimeSync;

namespace Annium.Finance.Providers.Crypto.Binance.Spot;

/// <summary>User-configurable timing settings for the Binance spot provider, with defaults suited to typical usage.</summary>
public sealed record ProviderConfiguration
{
    /// <summary>Gets how often a refused account-stream subscription is attempted again, in milliseconds.</summary>
    /// <remarks>
    /// The subscription is cheap and the socket it runs on is not, so a refusal is retried rather than
    /// reconnected around. Five seconds is the same order as the account's other recovery intervals: often
    /// enough that a transient refusal costs seconds, rare enough that a permanent one - a key without the
    /// permission, say - is a trickle rather than a hot loop.
    /// </remarks>
    public int SubscribeRetryInterval { get; init; } = 5_000;

    /// <summary>Gets the polling/timeout settings for the server time sync.</summary>
    public ServerTimeProviderConfig ServerTime { get; init; } = new(2_000, 5_000);

    /// <summary>Gets the schedule for reloading account balances.</summary>
    public CompositeLoaderConfig ReloadContext { get; init; } = new(1_000, 5, 3_000, 5_000, 5_000);

    /// <summary>Gets the schedule for reloading open orders.</summary>
    public CompositeLoaderConfig ReloadOrders { get; init; } = new(3_000, 5, 10_000, 60_000, 5_000);

    /// <summary>Gets the schedule for reloading recent trades.</summary>
    public CompositeLoaderConfig ReloadTrades { get; init; } = new(5_000, 5, 10_000, 0, 5_000);
}
