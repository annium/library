namespace Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;

/// <summary>
/// What a caller would do differently about a Binance error, which is the only distinction worth drawing
/// between several hundred numeric codes.
/// </summary>
/// <remarks>
/// <para>
/// Binance documents 206 error codes in five families. Mapping each one individually is not the useful
/// exercise - nobody writes a <c>switch</c> over 206 arms - and neither is the alternative this replaced,
/// where every negative code bar three became <c>BadRequest</c>. That answer told a caller its request was
/// malformed whether the key had expired, the order had already been cancelled, or the exchange was
/// shutting down for maintenance.
/// </para>
/// <para>
/// The classes here are actions, not categories: retry the same request, slow down, stop and fix
/// credentials, look somewhere else, reduce size, or accept that the exchange considered the request and
/// said no. Two codes belong in the same class exactly when a caller would treat them the same way.
/// </para>
/// </remarks>
internal enum BinanceErrorClass
{
    /// <summary>The code is not one we recognize, and not negative - so not a Binance error at all.</summary>
    Unknown,

    /// <summary>The request never completed for a transport reason, ours or the exchange's. Retryable as it stands.</summary>
    Transport,

    /// <summary>The request was abandoned before it finished.</summary>
    Aborted,

    /// <summary>A response arrived and could not be read.</summary>
    Unparsed,

    /// <summary>The exchange is refusing on volume. Slow down; the request itself was fine.</summary>
    RateLimited,

    /// <summary>The credentials are wrong, expired, locked or unwelcome from this address. No amount of retrying helps.</summary>
    Access,

    /// <summary>What was asked about does not exist: an unknown order, an unlisted symbol.</summary>
    NotFound,

    /// <summary>There is not enough balance or margin. Reduce size or fund the account.</summary>
    InsufficientFunds,

    /// <summary>The exchange understood the request, considered it, and declined it on its merits.</summary>
    Refused,
}
