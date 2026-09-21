using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User;

/// <summary>
/// Resolved user connector configuration (endpoints, credentials, listen key cadence) for USD-M futures, plus
/// the reload schedules for account context, orders and trades.
/// </summary>
internal sealed record UserConfig : UserConfigBase
{
    /// <summary>Gets the polling intervals used to fetch and keep the user data stream listen key alive.</summary>
    /// <remarks>
    /// Here rather than on the base: this venue reaches its account stream through a listen key and the other
    /// one does not, so a base carrying this makes that venue configure a mechanism it never resolves.
    /// </remarks>
    public required ListenKeyConfiguration ListenKey { get; init; }

    /// <summary>The reload schedule for the account context (assets and positions) loader.</summary>
    public required CompositeLoaderConfig ReloadContext { get; init; }

    /// <summary>The reload schedule for the open orders loader.</summary>
    public required CompositeLoaderConfig ReloadOrders { get; init; }

    /// <summary>The reload schedule for the trades loader.</summary>
    public required CompositeLoaderConfig ReloadTrades { get; init; }
}
