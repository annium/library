using Annium.Core.Mapper.Attributes;

namespace Annium.Finance.Providers.Abstractions.Domain.User.Operations;

/// <summary>
/// Represents the outcome status of a user (account/trading) provider operation.
/// </summary>
[AutoMapped]
public enum UserOperationStatus
{
    /// <summary>
    /// No outcome was recorded. Never produced by an operation: it is what a <see cref="UserResult"/> that
    /// nothing initialized reads as.
    /// </summary>
    /// <remarks>
    /// First, and therefore zero, on purpose. <see cref="UserResult"/> is a value type, so a field nobody
    /// assigned, an element of a fresh array and the <c>out</c> of a <c>TryRead</c> that returned false are
    /// all a valid instance rather than a null reference. With <see cref="Ok"/> at zero every one of those
    /// would read as a success carrying no data — the shape this codebase has met repeatedly, where
    /// something did not happen and the code carried on as though it had. Here they read as a failure,
    /// which is the direction that fails safe.
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

    /// <summary>The operation was rejected because the account is not authorized to perform it.</summary>
    Forbidden,

    /// <summary>The operation failed because the requested resource (order, position, etc.) was not found.</summary>
    NotFound,

    /// <summary>The operation failed because the provider's response could not be parsed.</summary>
    ParseError,

    /// <summary>The operation failed for a reason not covered by the other statuses.</summary>
    UnknownError,

    // custom statuses
    /// <summary>The operation was rejected because the account does not hold enough free balance to cover it.</summary>
    InsufficientBalance,
}
