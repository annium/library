using Annium.Core.Mapper.Attributes;

namespace Annium.Finance.Providers.Abstractions.Domain.Market.Operations;

/// <summary>
/// Represents the outcome status of a market data provider operation.
/// </summary>
[AutoMapped]
public enum MarketOperationStatus
{
    /// <summary>
    /// No outcome was recorded. Never produced by an operation: it is what a <see cref="MarketResult"/> that
    /// nothing initialized reads as.
    /// </summary>
    /// <remarks>
    /// First, and therefore zero, on purpose — see <c>UserOperationStatus.None</c> for the reasoning, which
    /// is the same on this half of the pair.
    /// </remarks>
    None,

    /// <summary>The operation completed successfully.</summary>
    Ok,

    /// <summary>The operation could not be performed because the provider is not connected.</summary>
    NotConnected,

    /// <summary>The operation failed because of a network-level error while communicating with the provider.</summary>
    NetworkError,

    /// <summary>The operation was aborted before it could complete.</summary>
    Aborted,

    /// <summary>The operation was rejected because the provider's rate limit was exceeded.</summary>
    TooManyRequests,

    /// <summary>The operation was rejected because the request was malformed or violated provider constraints.</summary>
    BadRequest,

    /// <summary>The operation failed because the requested resource (instrument, order, etc.) was not found.</summary>
    NotFound,

    /// <summary>The operation failed because the provider's response could not be parsed.</summary>
    ParseError,

    /// <summary>The operation failed for a reason not covered by the other statuses.</summary>
    UnknownError,
}
