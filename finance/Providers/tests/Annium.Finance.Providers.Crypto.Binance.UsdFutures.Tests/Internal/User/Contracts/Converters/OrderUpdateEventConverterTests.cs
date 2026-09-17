using System;
using System.Text;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User.Contracts.Converters;

/// <summary>
/// Verifies that <c>OrderUpdateEventConverter</c> reads Binance's <c>ORDER_TRADE_UPDATE</c> user-data stream
/// event - a nested <c>o</c> object covering both the order's cumulative state and its last individual fill -
/// into an <see cref="OrderUpdateEvent"/>, and that an event with a different <c>e</c> type deserializes to
/// null instead of throwing.
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
    /// Registers the Binance USD-M futures provider so the converter under test is resolved from its actual registration.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceUsdFutures();
    }

    /// <summary>
    /// A captured <c>ORDER_TRADE_UPDATE</c> event for a partially filled reduce-only trailing-stop order is
    /// parsed into the order's identifiers, cumulative executed quantity/price, the last individual fill,
    /// and commission.
    /// </summary>
    /// <remarks>
    /// Both status cases are here because <see cref="OrderUpdateEvent.CreatedAt"/> is not a field on this
    /// event - futures do not send one - but a value this converter synthesizes: the transaction time when
    /// the status is <c>NEW</c>, and zero otherwise, on the reasoning that the first event about an order
    /// is the moment it was created. Only the zero half used to be covered, so collapsing that condition in
    /// either direction went unnoticed. Spot needs none of this: its event carries <c>O</c>.
    /// </remarks>
    /// <param name="wireStatus">The <c>X</c> value to put in the payload.</param>
    /// <param name="status">The status it must parse to.</param>
    /// <param name="createdAt">The creation time the converter must synthesize for that status.</param>
    [Theory]
    [InlineData("PARTIALLY_FILLED", OrderStatus.PartiallyFilled, 0L)]
    [InlineData("NEW", OrderStatus.New, 1499405658657L)]
    public void Works(string wireStatus, OrderStatus status, long createdAt)
    {
        // arrange
        var raw =
            @"{
            ""e"": ""ORDER_TRADE_UPDATE"",
            ""E"": 1499405658678,
            ""T"": 1499405658677,
            ""o"": {
                ""s"": ""BTCUSDT"",
                ""c"": ""mUvoqJxFIILMdfAW5iGSOW"",
                ""S"": ""SELL"",
                ""o"": ""TRAILING_STOP_MARKET"",
                ""f"": ""GTC"",
                ""q"": ""1.7"",
                ""p"": ""10264.410"",
                ""ap"": ""12305.6"",
                ""sp"": ""7103.04"",
                ""x"": ""NEW"",
                ""X"": ""__STATUS__"",
                ""i"": 8886774,
                ""l"": ""2.4"",
                ""z"": ""10.5"",
                ""L"": ""11742.3"",
                ""N"": ""USDT"",
                ""n"": ""3.6"",
                ""T"": 1499405658657,
                ""t"": 123123,
                ""b"": ""0"",
                ""a"": ""9.91"",
                ""m"": true,
                ""R"": true,
                ""wt"": ""CONTRACT_PRICE"",
                ""ot"": ""TRAILING_STOP_MARKET"",
                ""ps"": ""LONG"",
                ""cp"": false,
                ""AP"": ""7476.89"",
                ""cr"": ""5.0"",
                ""pP"": false,
                ""si"": 0,
                ""ss"": 0,
                ""rp"": ""0""
            }
        }";

        // act - deserialize
        var serializer = this.GetJsonSerializer(Constants.OrderUpdateKey);
        var deserialized = serializer.Deserialize<OrderUpdateEvent>(
            Encoding.UTF8.GetBytes(raw.Replace("__STATUS__", wireStatus, StringComparison.Ordinal))
        );

        // assert - deserialization
        deserialized.Symbol.Is("BTCUSDT");
        deserialized.TradeId.Is("123123");
        deserialized.OrderId.Is("8886774");
        deserialized.ClientOrderId.Is("mUvoqJxFIILMdfAW5iGSOW");
        deserialized.Type.Is(OrderType.StopLossMarket);
        deserialized.Side.Is(OrderSide.Sell);
        deserialized.TotalQty.Is(1.7m);
        deserialized.Price.Is(10264.410m);
        deserialized.LevelPrice.Is(7103.04m);
        deserialized.ReduceOnly.IsTrue();
        deserialized.Status.Is(status);
        deserialized.ExecutedQty.Is(10.5m);
        deserialized.ExecutedPrice.Is(12305.6m);
        deserialized.LastExecutedQty.Is(2.4m);
        deserialized.LastExecutedPrice.Is(11742.3m);
        deserialized.CommissionAsset.Is("USDT");
        deserialized.CommissionAmount.Is(3.6m);
        deserialized.IsMaker.IsTrue();
        deserialized.CreatedAt.Is(createdAt);
        deserialized.UpdatedAt.Is(1499405658657);
    }

    /// <summary>
    /// An event whose <c>e</c> type tag isn't <c>ORDER_TRADE_UPDATE</c> deserializes to null instead of throwing.
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
}
