using System.Text;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User.Contracts.Domain;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User.Contracts.Converters;

/// <summary>
/// Verifies that <c>OrderUpdateEventConverter</c> reads Binance's <c>executionReport</c> user-data stream
/// event - a heavily abbreviated field set covering both the order's cumulative state and the last
/// individual fill - into an <see cref="OrderUpdateEvent"/>, and that an event with a different <c>e</c>
/// type deserializes to null instead of throwing.
/// </summary>
public class OrderUpdateEventConverterTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderUpdateEventConverterTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public OrderUpdateEventConverterTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the Binance Spot provider so the converter under test is resolved from its actual registration.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot();
    }

    /// <summary>
    /// A captured <c>executionReport</c> event is parsed into the order's identifiers, cumulative
    /// executed quantity/price, the last individual fill's quantity/price, and commission.
    /// </summary>
    [Fact]
    public void Works()
    {
        // arrange
        var raw =
            @"{
            ""e"": ""executionReport"",
            ""E"": 1499405658658,
            ""s"": ""ETHBTC"",
            ""c"": ""mUvoqJxFIILMdfAW5iGSOW"",
            ""S"": ""BUY"",
            ""o"": ""LIMIT"",
            ""f"": ""GTC"",
            ""q"": ""1.70000000"",
            ""p"": ""0.10264410"",
            ""P"": ""0.30000000"",
            ""F"": ""0.00000000"",
            ""g"": -1,
            ""C"": """",
            ""x"": ""NEW"",
            ""X"": ""NEW"",
            ""r"": ""NONE"",
            ""i"": 4293153,
            ""l"": ""2.10000000"",
            ""z"": ""1.20000000"",
            ""L"": ""3.30000000"",
            ""n"": ""5.4"",
            ""N"": ""BTC"",
            ""T"": 1499405658677,
            ""t"": 123123,
            ""I"": 8641984,
            ""w"": true,
            ""m"": true,
            ""M"": false,
            ""O"": 1499405658657,
            ""Z"": ""2.40000000"",
            ""Y"": ""0.00000000"",
            ""Q"": ""0.00000000"",
            ""W"": 1499405658657,
            ""V"": ""NONE""
        }";

        // act - deserialize
        var serializer = this.GetJsonSerializer(Constants.OrderUpdateKey);
        var deserialized = serializer.Deserialize<OrderUpdateEvent>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert - deserialization
        deserialized.Symbol.Is("ETHBTC");
        deserialized.TradeId.Is("123123");
        deserialized.OrderId.Is("4293153");
        deserialized.ClientOrderId.Is("mUvoqJxFIILMdfAW5iGSOW");
        deserialized.Type.Is(OrderType.Limit);
        deserialized.Side.Is(OrderSide.Buy);
        deserialized.TotalQty.Is(1.7m);
        deserialized.Price.Is(0.10264410m);
        deserialized.LevelPrice.Is(0.3m);
        deserialized.Status.Is(OrderStatus.New);
        deserialized.ExecutedQty.Is(1.2m);
        deserialized.ExecutedPrice.Is(2m);
        deserialized.LastExecutedQty.Is(2.1m);
        deserialized.LastExecutedPrice.Is(3.3m);
        deserialized.CommissionAsset.Is("BTC");
        deserialized.CommissionAmount.Is(5.4m);
        deserialized.IsMaker.IsTrue();
        deserialized.CreatedAt.Is(1499405658657);
        deserialized.UpdatedAt.Is(1499405658677);
    }

    /// <summary>
    /// An event whose <c>e</c> type tag isn't <c>executionReport</c> deserializes to null instead of throwing.
    /// </summary>
    [Fact]
    public void SkipsInvalidData()
    {
        // arrange
        var raw =
            @"{
            ""e"": ""invalid""
        }";

        // act - deserialize
        var serializer = this.GetJsonSerializer(Constants.OrderUpdateKey);
        var deserialized = serializer.Deserialize<OrderUpdateEvent>(Encoding.UTF8.GetBytes(raw));

        // assert - deserialization
        deserialized.IsDefault();
    }

    /// <summary>
    /// An event the exchange tags correctly but sends without an order id is dropped.
    /// </summary>
    /// <remarks>
    /// The drop joins two conditions - the wrong event tag, or no order id - and only the tag half was
    /// ever driven, by the case above. This is the other half, and it is the one that would arrive from
    /// a real stream rather than from a mismatched subscription.
    /// </remarks>
    [Fact]
    public void SkipsAnEventWithNoOrderId()
    {
        // arrange - a correctly tagged event whose order id never arrived
        var raw =
            @"{
            ""e"": ""executionReport"",
            ""E"": 1499405658658,
            ""s"": ""BTCUSDT"",
            ""c"": ""4a1f8bb3-724f-462e-9bde-6d0120381ddd"",
            ""S"": ""BUY"",
            ""o"": ""LIMIT"",
            ""f"": ""GTC"",
            ""q"": ""1.00000000"",
            ""p"": ""0.10264410"",
            ""X"": ""NEW"",
            ""z"": ""0.00000000"",
            ""Z"": ""0.00000000"",
            ""T"": 1499405658657,
            ""O"": 1499405658657
        }";

        // act
        var serializer = this.GetJsonSerializer(Constants.OrderUpdateKey);
        var deserialized = serializer.Deserialize<OrderUpdateEvent>(Encoding.UTF8.GetBytes(raw));

        // assert
        deserialized.IsDefault("an order update with no order id was handed over");
    }

    /// <summary>
    /// An order update reporting nothing filled prices it at zero rather than dividing by zero.
    /// </summary>
    /// <remarks>
    /// The third of the three converters carrying this guarded quotient, and the third whose fixtures
    /// all filled something. On the stream this arrives every time an order is placed.
    /// </remarks>
    [Fact]
    public void NothingFilled_IsAZeroPriceAndNotADivision()
    {
        // arrange
        var raw =
            @"{
            ""e"": ""executionReport"",
            ""E"": 1499405658658,
            ""s"": ""BTCUSDT"",
            ""c"": ""4a1f8bb3-724f-462e-9bde-6d0120381ddd"",
            ""S"": ""BUY"",
            ""o"": ""LIMIT"",
            ""f"": ""GTC"",
            ""q"": ""1.00000000"",
            ""p"": ""0.10264410"",
            ""X"": ""NEW"",
            ""i"": 4293153,
            ""z"": ""0.00000000"",
            ""Z"": ""0.00000000"",
            ""T"": 1499405658657,
            ""O"": 1499405658657
        }";

        // act
        var serializer = this.GetJsonSerializer(Constants.OrderUpdateKey);
        var deserialized = serializer.Deserialize<OrderUpdateEvent>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        deserialized.ExecutedQty.Is(0m);
        deserialized.ExecutedPrice.Is(0m, "an order update with no fills priced them");
    }

    /// <summary>
    /// The event the venue sends when an order is accepted carries a null commission asset, and reads as
    /// no commission rather than throwing.
    /// </summary>
    /// <remarks>
    /// Recorded off the wire on 2026-09-21, the first time anything on this venue was made to fill. Every
    /// fixture here until then was assembled by hand from the documentation, and each of them spelled the
    /// commission asset as a string and the amount as a padded decimal — so `null` and the bare `"0"` the
    /// venue actually sends had never been read. Both are in this payload exactly as it arrived.
    /// </remarks>
    [Fact]
    public void TheAcceptanceEvent_CarriesNoCommissionAsset()
    {
        // arrange - the NEW event of a market buy, verbatim
        var raw =
            @"{
            ""e"":""executionReport"",""E"":1789988980861,""s"":""DOTUSDT"",
            ""c"":""e08e515ba11f4c8bbb88be28ac3045f7"",""S"":""BUY"",""o"":""MARKET"",""f"":""GTC"",
            ""q"":""5.85000000"",""p"":""0.00000000"",""P"":""0.00000000"",""F"":""0.00000000"",""g"":-1,
            ""C"":"""",""x"":""NEW"",""X"":""NEW"",""r"":""NONE"",""i"":6205396248,""l"":""0.00000000"",
            ""z"":""0.00000000"",""L"":""0.00000000"",""n"":""0"",""N"":null,""T"":1789988980861,""t"":-1,
            ""I"":12853160421,""w"":true,""m"":false,""M"":false,""O"":1789988980861,""Z"":""0.00000000"",
            ""Y"":""0.00000000"",""Q"":""0.00000000"",""W"":1789988980861,""V"":""EXPIRE_MAKER""
        }";

        // act
        var serializer = this.GetJsonSerializer(Constants.OrderUpdateKey);
        var deserialized = serializer.Deserialize<OrderUpdateEvent>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        deserialized.Status.Is(OrderStatus.New);
        deserialized.CommissionAsset.Is(string.Empty, "a null commission asset must read as none");
        deserialized.CommissionAmount.Is(0m);
        deserialized.ExecutedQty.Is(0m);
    }

    /// <summary>
    /// A fill is priced by what it cost, not by what the order asked for.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The event of a limit buy that crossed the book, recorded off the wire on 2026-09-21. It is the
    /// payload that distinguishes the two readings: the order was priced at 1.202 and filled at 1.197,
    /// because a marketable limit takes the book's price rather than its own. A converter reading the
    /// order's price as its execution price passes every fixture where the two agree — which is every
    /// fixture a resting order produces.
    /// </para>
    /// <para>
    /// It also pins that the fill arrives as its own event: the acceptance carries <c>NEW</c> and this one
    /// carries the execution, so a caller that reads only the first of the two sees an order that was
    /// accepted and never filled.
    /// </para>
    /// </remarks>
    [Fact]
    public void AFill_IsPricedByWhatItCostAndNotByTheOrdersPrice()
    {
        // arrange - the TRADE event of a limit buy priced through the book, verbatim
        var raw =
            @"{
            ""e"":""executionReport"",""E"":1789989040123,""s"":""DOTUSDT"",
            ""c"":""9c1d0b4a0c1a4a1bb1a0a7f9a3c2d5e1"",""S"":""BUY"",""o"":""LIMIT"",""f"":""GTC"",
            ""q"":""5.85000000"",""p"":""1.20200000"",""P"":""0.00000000"",""F"":""0.00000000"",""g"":-1,
            ""C"":"""",""x"":""TRADE"",""X"":""FILLED"",""r"":""NONE"",""i"":6205396571,
            ""l"":""5.85000000"",""z"":""5.85000000"",""L"":""1.19700000"",""n"":""0.00585000"",""N"":""DOT"",
            ""T"":1789989040122,""t"":446186485,""I"":12853160900,""w"":false,""m"":false,""M"":true,
            ""O"":1789989040122,""Z"":""7.00245000"",""Y"":""7.00245000"",""Q"":""0.00000000"",
            ""W"":1789989040122,""V"":""EXPIRE_MAKER""
        }";

        // act
        var serializer = this.GetJsonSerializer(Constants.OrderUpdateKey);
        var deserialized = serializer.Deserialize<OrderUpdateEvent>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        deserialized.Status.Is(OrderStatus.Filled);
        deserialized.Price.Is(1.202m, "the price the order was placed at");
        deserialized.ExecutedPrice.Is(
            1.197m,
            "the execution was priced by the order rather than by what it actually cost"
        );
        deserialized.ExecutedQty.Is(5.85m);
        deserialized.CommissionAmount.Is(0.00585m);
        deserialized.CommissionAsset.Is("DOT", "the fee on a buy is taken in the asset bought");
    }
}
