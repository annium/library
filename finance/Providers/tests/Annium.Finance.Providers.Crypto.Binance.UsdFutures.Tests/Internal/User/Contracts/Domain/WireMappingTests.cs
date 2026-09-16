using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User.Contracts.Domain;

/// <summary>
/// Pins the tables that translate between this module's domain enums and Binance's USD-M wire values.
/// </summary>
/// <remarks>
/// <para>
/// Five tables, and until now not one of them had a test. What covered them was incidental: a value was
/// exercised if some converter fixture happened to contain it, so on the read side only <c>NEW</c>,
/// <c>PARTIALLY_FILLED</c> and <c>CANCELED</c> were ever parsed — no fixture in the assembly carried
/// <c>FILLED</c>, <c>REJECTED</c>, <c>EXPIRED</c> or <c>EXPIRED_IN_MATCH</c>, the four that say an order
/// is finished.
/// </para>
/// <para>
/// That gap has teeth. Both connectors decide whether an order has left the book by reading
/// <c>order.Status is not (New or PartiallyFilled)</c>, so a status that parses to the wrong member, or
/// throws because it parses to nothing, is a filled order the caller goes on treating as open.
/// </para>
/// <para>
/// The assertions are about the tables as a whole rather than about chosen examples. Listing a few pairs
/// proves those pairs and says nothing about the member somebody adds next year — and adding a member to
/// the domain enum without extending the map is the failure worth catching, because `MapValue` throws on
/// a real response and the test suite never notices.
/// </para>
/// </remarks>
public class WireMappingTests
{
    /// <summary>
    /// Every status Binance documents for USD-M parses, including the four no fixture ever carried.
    /// </summary>
    /// <param name="wire">The <c>status</c> value as Binance sends it.</param>
    /// <param name="expected">The status it must parse to.</param>
    [Theory]
    [InlineData("NEW", OrderStatus.New)]
    [InlineData("PARTIALLY_FILLED", OrderStatus.PartiallyFilled)]
    [InlineData("FILLED", OrderStatus.Filled)]
    [InlineData("CANCELED", OrderStatus.Canceled)]
    [InlineData("REJECTED", OrderStatus.Rejected)]
    [InlineData("EXPIRED", OrderStatus.Expired)]
    [InlineData("EXPIRED_IN_MATCH", OrderStatus.Rejected)]
    public void OrderStatus_ParsesEveryDocumentedValue(string wire, OrderStatus expected)
    {
        OrderStatuses.StringToValue.ContainsKey(wire).IsTrue($"no mapping for status '{wire}'");
        OrderStatuses.StringToValue[wire].Is(expected);
    }

    /// <summary>
    /// The four statuses that mean the order is finished parse to members the domain reads as terminal,
    /// which is the question both connectors actually ask of this table.
    /// </summary>
    /// <param name="wire">A <c>status</c> value that means the order is no longer working.</param>
    [Theory]
    [InlineData("FILLED")]
    [InlineData("CANCELED")]
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
    /// Every domain status can be written back out, so a member added to the enum cannot be left without
    /// a wire value.
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
    /// Every order type Binance documents for USD-M parses, <c>TRAILING_STOP_MARKET</c> included.
    /// </summary>
    /// <param name="wire">The <c>type</c> value as Binance sends it.</param>
    /// <param name="expected">The type it must parse to.</param>
    [Theory]
    [InlineData("LIMIT", OrderType.Limit)]
    [InlineData("MARKET", OrderType.Market)]
    [InlineData("STOP", OrderType.StopLossLimit)]
    [InlineData("STOP_MARKET", OrderType.StopLossMarket)]
    [InlineData("TAKE_PROFIT", OrderType.TakeProfitLimit)]
    [InlineData("TAKE_PROFIT_MARKET", OrderType.TakeProfitMarket)]
    [InlineData("TRAILING_STOP_MARKET", OrderType.StopLossMarket)]
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
    /// The two folds this venue's tables carry: a wire value that parses in but is never written out,
    /// because two of Binance's values mean one thing to this domain.
    /// </summary>
    /// <remarks>
    /// Stated on purpose. Read as an accident, either looks like an asymmetry worth tidying away, and
    /// removing it turns an order the exchange really does report into one that fails to parse.
    /// </remarks>
    [Fact]
    public void Folds_ParseInWithoutBeingWrittenOut()
    {
        // an order the exchange expired under self-trade prevention: there is no distinct domain status,
        // and rejected is the honest reading of "the exchange refused to let it trade"
        OrderStatuses.StringToValue["EXPIRED_IN_MATCH"].Is(OrderStatus.Rejected);
        OrderStatuses.ValueToString.Values.Contains("EXPIRED_IN_MATCH").IsFalse();

        // a trailing stop is a stop this domain cannot express the trailing part of; it still has to be
        // readable, because the exchange will report one placed elsewhere
        OrderTypes.StringToValue["TRAILING_STOP_MARKET"].Is(OrderType.StopLossMarket);
        OrderTypes.ValueToString.Values.Contains("TRAILING_STOP_MARKET").IsFalse();
    }

    /// <summary>
    /// Sides, orientation ranges and margin types: small tables, mapped both ways in full.
    /// </summary>
    [Fact]
    public void SmallTables_MapEveryMemberBothWays()
    {
        AssertEveryMemberMapped(OrderSides.ValueToString);
        AssertRoundTrips(OrderSides.ValueToString, OrderSides.StringToValue);

        AssertEveryMemberMapped(OrientationRanges.ValueToString);
        AssertRoundTrips(OrientationRanges.ValueToString, OrientationRanges.StringToValue);

        AssertEveryMemberMapped(MarginTypes.ValueToString);
        AssertRoundTrips(MarginTypes.ValueToString, MarginTypes.StringToValue);
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
