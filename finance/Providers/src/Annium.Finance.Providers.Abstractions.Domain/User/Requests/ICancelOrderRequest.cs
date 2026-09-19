namespace Annium.Finance.Providers.Abstractions.Domain.User.Requests;

/// <summary>
/// Represents a request to cancel a previously placed order.
/// </summary>
public interface ICancelOrderRequest
{
    /// <summary>Gets the provider-assigned identifier of the order to cancel.</summary>
    string Id { get; }

    /// <summary>Gets the client-assigned identifier of the order to cancel.</summary>
    string ClientOrderId { get; }

    /// <summary>Gets the instrument symbol the order to cancel belongs to.</summary>
    string Symbol { get; }

    /// <summary>Gets the type of the order to cancel.</summary>
    /// <remarks>
    /// Carried because a provider may keep order types in separate stores, reached by separate endpoints -
    /// and then a cancellation has to know which one to address. The alternative is for the provider to look
    /// the order up in whatever it has cached, which makes cancelling correctly depend on a cache being
    /// current; a caller that is cancelling an order it placed always knows its type.
    /// </remarks>
    OrderType Type { get; }
}
