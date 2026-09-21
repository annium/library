using System;
using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Core.Shared.TimeSync;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures;

/// <summary>
/// User-facing configuration for the Binance USD-M futures provider: listen key keep-alive, server time sync,
/// and the reload schedules for account context, orders and trades. Passed to
/// <see cref="ProviderRegistrationContextExtensions"/>'s registration method; defaults are used when registering
/// without an explicit configuration.
/// </summary>
public sealed record ProviderConfiguration
{
    /// <summary>Gets the base URI of the account/trading HTTP API, or null to use the exchange's own.</summary>
    /// <remarks>
    /// The exchange publishes a testnet with its own hosts, and a caller that wants one is configuring an
    /// endpoint rather than patching a constant. Each of the three is overridden on its own, because the
    /// reasons to move them do not arrive together: a testnet moves all three, while a test that needs to
    /// cut a connection moves only the socket it means to cut and leaves the rest pointed at the venue.
    /// </remarks>
    public Uri? HttpApi { get; init; }

    /// <summary>Gets the base URI of the market data stream, or null to use the exchange's own.</summary>
    public Uri? MarketWsApi { get; init; }

    /// <summary>Gets the base URI the account stream is opened on, or null to use the exchange's own.</summary>
    /// <remarks>
    /// The same host as the market stream on this market type, and a separate setting all the same: the two
    /// are one endpoint by the venue's choice rather than by ours, and a test relaying the account stream
    /// must not silently relay market data with it.
    /// </remarks>
    public Uri? UserWsApi { get; init; }

    /// <summary>The listen key ping interval and expiration handling.</summary>
    public ListenKeyConfiguration ListenKey { get; init; } = new(5_000, 60_000);

    /// <summary>The server time sync interval and staleness tolerance.</summary>
    public ServerTimeProviderConfig ServerTime { get; init; } = new(2_000, 5_000);

    /// <summary>The reload schedule for the account context (assets and positions) loader.</summary>
    public CompositeLoaderConfig ReloadContext { get; init; } = new(1_000, 5, 3_000, 5_000, 5_000);

    /// <summary>The reload schedule for the open orders loader.</summary>
    public CompositeLoaderConfig ReloadOrders { get; init; } = new(3_000, 5, 10_000, 60_000, 5_000);

    /// <summary>The reload schedule for the trades loader.</summary>
    public CompositeLoaderConfig ReloadTrades { get; init; } = new(5_000, 5, 10_000, 0, 5_000);
}
