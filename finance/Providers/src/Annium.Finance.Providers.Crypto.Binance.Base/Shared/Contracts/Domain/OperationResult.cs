namespace Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;

/// <summary>A Binance error response, as returned in the body of a failed HTTP request or WebSocket message.</summary>
/// <param name="Code">The Binance error code, negative for exchange-reported errors, or one of the local synthetic codes (<see cref="NetworkError"/>, <see cref="Aborted"/>, <see cref="ParseError"/>) for transport-level failures.</param>
/// <param name="Message">The human-readable error message.</param>
public sealed record OperationResult(long Code, string Message)
{
    /// <summary>Synthetic code used when the request could not be sent because of a network-level failure.</summary>
    public const long NetworkError = 1;

    /// <summary>Synthetic code used when the request was aborted before completing.</summary>
    public const long Aborted = 2;

    /// <summary>Synthetic code used when the response could not be parsed.</summary>
    public const long ParseError = 3;

    /// <summary>Binance's own code for a request rejected because too many were sent, including an IP ban.</summary>
    public const long TooManyRequests = -1003;

    /// <summary>Whether this result was made up locally rather than read from a Binance response.</summary>
    /// <remarks>
    /// A synthetic result says what stopped us from reading the body, which is a poorer answer than an HTTP
    /// status the server did send - so where both exist, the status is the one to map from.
    /// </remarks>
    public bool IsSynthetic => Code is NetworkError or Aborted or ParseError;
}
