using System.Collections.Generic;
using Annium.Finance.Providers.Abstractions.Domain.User;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;

/// <summary>
/// Maps Binance's <c>algoStatus</c> wire values onto <see cref="OrderStatus"/>, for the conditional orders
/// that moved to the algo endpoints. Two of the seven deliberately map to nothing.
/// </summary>
/// <remarks>
/// <para>
/// The exchange publishes seven statuses against this library's six, and <c>FINISHED</c> is documented as
/// "filled <em>or</em> canceled" - so on its own it is not a status at all. What resolves it is not a
/// reading but a measurement: a conditional order that triggers becomes an **ordinary order**, and that
/// order carries the same client id we sent to the algo endpoint. Identity survives the trigger; only the
/// store changes.
/// </para>
/// <para>
/// So <c>TRIGGERED</c> and <c>FINISHED</c> map to nothing on purpose. A record in either state is a
/// duplicate of an ordinary order that already says what happened, under the same id, and surfacing both
/// would show a caller one order twice - once with a real fill and once with a guess. Mapping
/// <c>FINISHED</c> to <see cref="OrderStatus.Filled"/> was the obvious shortcut and is wrong every time the
/// triggered order is cancelled in the book.
/// </para>
/// </remarks>
internal static class AlgoStatuses
{
    /// <summary>Maps each <c>algoStatus</c> wire value that has a domain meaning to its <see cref="OrderStatus"/>.</summary>
    public static readonly IReadOnlyDictionary<string, OrderStatus> StringToValue;

    /// <summary>
    /// The <c>algoStatus</c> values that carry no domain status, because an ordinary order with the same id
    /// carries the outcome instead.
    /// </summary>
    public static readonly IReadOnlySet<string> Superseded;

    /// <summary>Initializes the lookup tables.</summary>
    static AlgoStatuses()
    {
        StringToValue = new Dictionary<string, OrderStatus>
        {
            // placed and waiting for its trigger; the algo store is the only place it exists
            { "NEW", OrderStatus.New },
            // the trigger fired and the order is on its way to the matching engine - still not a book order
            { "TRIGGERING", OrderStatus.New },
            { "CANCELED", OrderStatus.Canceled },
            { "REJECTED", OrderStatus.Rejected },
            { "EXPIRED", OrderStatus.Expired },
        };

        Superseded = new HashSet<string> { "TRIGGERED", "FINISHED" };
    }

    /// <summary>Reads a wire status, saying both whether it is known and whether it has a domain meaning.</summary>
    /// <param name="wire">The <c>algoStatus</c> value received.</param>
    /// <param name="status">The domain status, when there is one.</param>
    /// <returns>
    /// <see langword="true"/> when the value maps to a domain status; <see langword="false"/> when it is
    /// superseded by an ordinary order or is not recognized at all.
    /// </returns>
    public static bool TryMap(string wire, out OrderStatus status) => StringToValue.TryGetValue(wire, out status);
}
