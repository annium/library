using System.Collections.Generic;
using Annium.Finance.Providers.Abstractions.Domain.User;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User.Contracts.Domain;

/// <summary>
/// Maps between the library's <see cref="OrderStatus"/> and the Binance <c>status</c> string values. On the
/// read side, <c>PENDING_CANCEL</c> is folded into <see cref="OrderStatus.Canceled"/>,
/// <c>PENDING_NEW</c> into <see cref="OrderStatus.New"/> and <c>EXPIRED_IN_MATCH</c> into
/// <see cref="OrderStatus.Rejected"/>.
/// </summary>
internal static class OrderStatuses
{
    /// <summary>Maps an <see cref="OrderStatus"/> to its Binance wire representation.</summary>
    public static readonly IReadOnlyDictionary<OrderStatus, string> ValueToString;

    /// <summary>Maps a Binance <c>status</c> string to the corresponding <see cref="OrderStatus"/>.</summary>
    public static readonly IReadOnlyDictionary<string, OrderStatus> StringToValue;

    /// <summary>Initializes the static lookup tables.</summary>
    static OrderStatuses()
    {
        ValueToString = new Dictionary<OrderStatus, string>
        {
            { OrderStatus.New, "NEW" },
            { OrderStatus.PartiallyFilled, "PARTIALLY_FILLED" },
            { OrderStatus.Filled, "FILLED" },
            { OrderStatus.Canceled, "CANCELED" },
            { OrderStatus.Rejected, "REJECTED" },
            { OrderStatus.Expired, "EXPIRED" },
        };

        StringToValue = new Dictionary<string, OrderStatus>
        {
            { "NEW", OrderStatus.New },
            { "PARTIALLY_FILLED", OrderStatus.PartiallyFilled },
            { "FILLED", OrderStatus.Filled },
            { "CANCELED", OrderStatus.Canceled },
            { "PENDING_CANCEL", OrderStatus.Canceled },
            // an order in an order list waits in this state until its working order fills: it exists,
            // it is live, and nothing of it is filled - which is what New says. Unmapped until now,
            // and that was not an omission anyone chose: the lookup throws on a value it does not
            // know, so one pending leg would have failed the parse of the whole list it arrived in
            { "PENDING_NEW", OrderStatus.New },
            { "REJECTED", OrderStatus.Rejected },
            { "EXPIRED", OrderStatus.Expired },
            { "EXPIRED_IN_MATCH", OrderStatus.Rejected },
        };
    }
}
