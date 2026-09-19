namespace Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;

/// <summary>Classifies Binance error codes by what a caller would do about them.</summary>
/// <remarks>
/// One table, consulted by both the user and market result mappings. They used to carry a code list each,
/// and the copies drifted: a new code was added to one and not the other, and nothing noticed for months
/// because each copy passed its own tests. Sharing the classification makes that divergence impossible
/// rather than merely discouraged - the two mappings still differ, but only in how they *spell* a class,
/// which is the part that genuinely differs between them.
/// </remarks>
internal static class BinanceErrors
{
    /// <summary>Classifies a Binance error code, or one of the local synthetic codes.</summary>
    /// <param name="code">The code carried in the error payload.</param>
    /// <returns>The class the code belongs to.</returns>
    /// <remarks>
    /// Codes are cited by the name Binance gives them, so this table can be checked against
    /// <c>error-code.md</c> in the contract snapshot without guessing what a number meant. Anything negative
    /// and unlisted is <see cref="BinanceErrorClass.Refused"/>: the exchange answered with an error of its
    /// own, so it read the request and declined it, which is a far better default than calling it malformed.
    /// </remarks>
    public static BinanceErrorClass Classify(long code) =>
        code switch
        {
            // local, synthetic - nothing reached the exchange, or nothing came back readable
            OperationResult.NetworkError => BinanceErrorClass.Transport,
            OperationResult.Aborted => BinanceErrorClass.Aborted,
            OperationResult.ParseError => BinanceErrorClass.Unparsed,

            // the exchange could not complete the request for its own transport reasons. Retryable as-is,
            // which is what separates these from everything else negative
            -1001 => BinanceErrorClass.Transport, // DISCONNECTED
            -1007 => BinanceErrorClass.Transport, // TIMEOUT
            -1016 => BinanceErrorClass.Transport, // SERVICE_SHUTTING_DOWN

            // volume, not content
            -1003 => BinanceErrorClass.RateLimited, // TOO_MANY_REQUESTS
            -1008 => BinanceErrorClass.RateLimited, // Request Throttled
            -1015 => BinanceErrorClass.RateLimited, // TOO_MANY_ORDERS

            // credentials. Every one of these used to arrive as BadRequest, so an expired key was
            // indistinguishable from a malformed price - and the two want opposite responses
            -1002 => BinanceErrorClass.Access, // UNAUTHORIZED
            -1011 => BinanceErrorClass.Access, // NON_WHITE_LIST
            -2014 => BinanceErrorClass.Access, // BAD_API_KEY_FMT
            -2015 => BinanceErrorClass.Access, // REJECTED_MBX_KEY
            -2017 => BinanceErrorClass.Access, // API_KEYS_LOCKED

            -1099 => BinanceErrorClass.NotFound, // NOT_FOUND
            -1121 => BinanceErrorClass.NotFound, // BAD_SYMBOL
            -2013 => BinanceErrorClass.NotFound, // NO_SUCH_ORDER

            -2018 => BinanceErrorClass.InsufficientFunds, // BALANCE_NOT_SUFFICIENT
            -2019 => BinanceErrorClass.InsufficientFunds, // MARGIN_NOT_SUFFICIENT

            < 0 => BinanceErrorClass.Refused,
            _ => BinanceErrorClass.Unknown,
        };
}
