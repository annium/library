using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User.Contracts.Domain;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User.Contracts.Domain;

/// <summary>
/// Pins the tables that translate between this module's domain enums and Binance's spot wire values.
/// </summary>
/// <remarks>
/// <para>
/// The USD-M half of this pair was written first; this one exists because the rule in this repository is
/// that a fix landing on one half of a symmetric pair makes the other a finding until proven otherwise.
/// It was — spot had no test for any of its three tables either.
/// </para>
/// <para>
/// Spot's values are not USD-M's, and the differences are the interesting part: it reports
/// <c>STOP_LOSS</c> where futures report <c>STOP_MARKET</c>, and it has two folds of its own —
/// <c>PENDING_CANCEL</c>, a cancellation still in flight, and <c>LIMIT_MAKER</c>, a post-only limit this
/// domain has no separate member for. A test copied across venues without re-reading the tables would
/// have passed on the wrong strings.
/// </para>
/// </remarks>
public class WireMappingTests
{
    /// <summary>
    /// Every status Binance documents for spot parses, including the ones no fixture carries.
    /// </summary>
    /// <param name="wire">The <c>status</c> value as Binance sends it.</param>
    /// <param name="expected">The status it must parse to.</param>
    [Theory]
    [InlineData("NEW", OrderStatus.New)]
    [InlineData("PARTIALLY_FILLED", OrderStatus.PartiallyFilled)]
    [InlineData("FILLED", OrderStatus.Filled)]
    [InlineData("CANCELED", OrderStatus.Canceled)]
    [InlineData("PENDING_CANCEL", OrderStatus.Canceled)]
    [InlineData("REJECTED", OrderStatus.Rejected)]
    [InlineData("EXPIRED", OrderStatus.Expired)]
    [InlineData("EXPIRED_IN_MATCH", OrderStatus.Rejected)]
    public void OrderStatus_ParsesEveryDocumentedValue(string wire, OrderStatus expected)
    {
        OrderStatuses.StringToValue.ContainsKey(wire).IsTrue($"no mapping for status '{wire}'");
        OrderStatuses.StringToValue[wire].Is(expected);
    }

    /// <summary>
    /// The statuses that mean the order is finished parse to members the domain reads as terminal.
    /// </summary>
    /// <remarks>
    /// <c>PENDING_CANCEL</c> is in this list deliberately: it maps to <c>Canceled</c>, so as far as every
    /// consumer is concerned the order has left the book while the exchange still has the cancellation in
    /// flight. That is a decision, not an oversight, and this is where it is visible.
    /// </remarks>
    /// <param name="wire">A <c>status</c> value that means the order is no longer working.</param>
    [Theory]
    [InlineData("FILLED")]
    [InlineData("CANCELED")]
    [InlineData("PENDING_CANCEL")]
    [InlineData("REJECTED")]
    [InlineData("EXPIRED")]
    [InlineData("EXPIRED_IN_MATCH")]
    public void OrderStatus_TerminalValues_AreNotOpen(string wire)
    {
        var status = OrderStatuses.StringToValue[wire];

        (status is OrderStatus.New or OrderStatus.PartiallyFilled).IsFalse(
            $"'{wire}' parsed to {status}, which the connectors read as still open"
        );
    }

    /// <summary>
    /// Every domain status can be written back out.
    /// </summary>
    [Fact]
    public void OrderStatus_EveryDomainMemberIsWritable()
    {
        AssertEveryMemberMapped(OrderStatuses.ValueToString);
    }

    /// <summary>
    /// Writing a status out and reading it back returns the same member.
    /// </summary>
    [Fact]
    public void OrderStatus_RoundTrips()
    {
        AssertRoundTrips(OrderStatuses.ValueToString, OrderStatuses.StringToValue);
    }

    /// <summary>
    /// Every order type Binance documents for spot parses — note these are not the futures strings.
    /// </summary>
    /// <param name="wire">The <c>type</c> value as Binance sends it.</param>
    /// <param name="expected">The type it must parse to.</param>
    [Theory]
    [InlineData("LIMIT", OrderType.Limit)]
    [InlineData("MARKET", OrderType.Market)]
    [InlineData("STOP_LOSS", OrderType.StopLossMarket)]
    [InlineData("STOP_LOSS_LIMIT", OrderType.StopLossLimit)]
    [InlineData("TAKE_PROFIT", OrderType.TakeProfitMarket)]
    [InlineData("TAKE_PROFIT_LIMIT", OrderType.TakeProfitLimit)]
    [InlineData("LIMIT_MAKER", OrderType.Limit)]
    public void OrderType_ParsesEveryDocumentedValue(string wire, OrderType expected)
    {
        OrderTypes.StringToValue.ContainsKey(wire).IsTrue($"no mapping for type '{wire}'");
        OrderTypes.StringToValue[wire].Is(expected);
    }

    /// <summary>
    /// Every domain order type can be written back out.
    /// </summary>
    [Fact]
    public void OrderType_EveryDomainMemberIsWritable()
    {
        AssertEveryMemberMapped(OrderTypes.ValueToString);
    }

    /// <summary>
    /// Writing an order type out and reading it back returns the same member.
    /// </summary>
    [Fact]
    public void OrderType_RoundTrips()
    {
        AssertRoundTrips(OrderTypes.ValueToString, OrderTypes.StringToValue);
    }

    /// <summary>
    /// Spot's three folds: wire values that parse in and are never written out.
    /// </summary>
    /// <remarks>
    /// Stated on purpose, because each reads as a tidy-up waiting to happen — and removing one turns an
    /// order the exchange really does report into one that fails to parse. <c>LIMIT_MAKER</c> is the
    /// costly one to lose: a post-only limit is an ordinary thing to place, and this domain has no
    /// separate member for it, so it arrives as a plain limit or not at all.
    /// </remarks>
    [Fact]
    public void Folds_ParseInWithoutBeingWrittenOut()
    {
        OrderStatuses.StringToValue["PENDING_CANCEL"].Is(OrderStatus.Canceled);
        OrderStatuses.ValueToString.Values.Contains("PENDING_CANCEL").IsFalse();

        OrderStatuses.StringToValue["EXPIRED_IN_MATCH"].Is(OrderStatus.Rejected);
        OrderStatuses.ValueToString.Values.Contains("EXPIRED_IN_MATCH").IsFalse();

        OrderTypes.StringToValue["LIMIT_MAKER"].Is(OrderType.Limit);
        OrderTypes.ValueToString.Values.Contains("LIMIT_MAKER").IsFalse();
    }

    /// <summary>
    /// Sides: mapped both ways in full.
    /// </summary>
    [Fact]
    public void OrderSide_MapsEveryMemberBothWays()
    {
        AssertEveryMemberMapped(OrderSides.ValueToString);
        AssertRoundTrips(OrderSides.ValueToString, OrderSides.StringToValue);
    }

    /// <summary>
    /// Asserts the table names every member of its domain enum.
    /// </summary>
    /// <typeparam name="T">The domain enum.</typeparam>
    /// <param name="valueToString">The outbound table.</param>
    private static void AssertEveryMemberMapped<T>(IReadOnlyDictionary<T, string> valueToString)
        where T : struct, Enum
    {
        var missing = Enum.GetValues<T>().Where(x => !valueToString.ContainsKey(x)).ToArray();

        missing.IsEmpty($"{typeof(T).Name} members with no wire value: {string.Join(", ", missing)}");
    }

    /// <summary>
    /// Asserts that everything written out reads back as the member it was written from.
    /// </summary>
    /// <typeparam name="T">The domain enum.</typeparam>
    /// <param name="valueToString">The outbound table.</param>
    /// <param name="stringToValue">The inbound table.</param>
    private static void AssertRoundTrips<T>(
        IReadOnlyDictionary<T, string> valueToString,
        IReadOnlyDictionary<string, T> stringToValue
    )
        where T : struct, Enum
    {
        foreach (var (value, wire) in valueToString)
        {
            stringToValue.ContainsKey(wire).IsTrue($"{typeof(T).Name}.{value} writes '{wire}', which does not parse");
            stringToValue[wire]
                .Is(value, $"{typeof(T).Name}.{value} writes '{wire}', which parses back as something else");
        }
    }
}
