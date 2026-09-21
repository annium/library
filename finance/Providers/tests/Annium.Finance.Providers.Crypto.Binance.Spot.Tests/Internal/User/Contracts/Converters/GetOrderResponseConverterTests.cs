using System;
using System.Text;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User.Contracts.Converters;

/// <summary>
/// Verifies that <c>GetOrderResponseConverter</c> reads Binance's <c>GET /order</c> response into an
/// <see cref="OrderModel"/>, in particular deriving the average executed price from
/// <c>cummulativeQuoteQty / executedQty</c> since Binance doesn't report it directly.
/// </summary>
public class GetOrderResponseConverterTests : ProvidersTestBase
{
    /// <summary>
    /// A recorded answer to a get-order request, carried as one fixture so a variant of it is a change
    /// to this payload rather than a second payload somebody wrote from imagination.
    /// </summary>
    private const string Raw =
        @" {
            ""symbol"": ""BTCUSDT"",
            ""orderId"": 28,
            ""orderListId"": -1,
            ""clientOrderId"": ""4a1f8bb3-724f-462e-9bde-6d0120381ddd"",
            ""price"": ""10019.7"",
            ""origQty"": ""10.5"",
            ""executedQty"": ""6.4"",
            ""cummulativeQuoteQty"": ""12.8"",
            ""status"": ""PARTIALLY_FILLED"",
            ""timeInForce"": ""GTC"",
            ""type"": ""TAKE_PROFIT_LIMIT"",
            ""side"": ""SELL"",
            ""stopPrice"": ""10015.5"",
            ""icebergQty"": ""0.0"",
            ""time"": 1499827319559,
            ""updateTime"": 1499827319579,
            ""isWorking"": true,
            ""origQuoteOrderQty"": ""0.000000"",
            ""workingTime"": 1499827319559,
            ""selfTradePreventionMode"": ""NONE""
        }";

    /// <summary>
    /// Initializes a new instance of the <see cref="GetOrderResponseConverterTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public GetOrderResponseConverterTests(ITestOutputHelper outputHelper)
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
    /// A captured order response for a partially filled take-profit-limit sell is parsed into an
    /// <see cref="OrderModel"/>, including the executed price derived from cumulative quote quantity
    /// divided by executed quantity.
    /// </summary>
    [Fact]
    public void Works()
    {
        // arrange
        var raw = Raw;

        // act
        var serializer = this.GetJsonSerializer(Constants.GetOrderKey);
        var deserialized = serializer.Deserialize<OrderModel>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        deserialized.Id.Is("28");
        deserialized.ClientOrderId.Is("4a1f8bb3-724f-462e-9bde-6d0120381ddd");
        deserialized.Symbol.Is("BTCUSDT");
        deserialized.Side.Is(OrderSide.Sell);
        deserialized.Type.Is(OrderType.TakeProfitLimit);
        deserialized.TotalQty.Is(10.5m);
        deserialized.Price.Is(10019.7m);
        deserialized.LevelPrice.Is(10015.5m);
        deserialized.CreatedAt.Is(1499827319559);
        deserialized.Status.Is(OrderStatus.PartiallyFilled);
        deserialized.ExecutedQty.Is(6.4m);
        deserialized.ExecutedPrice.Is(2m);
        deserialized.UpdatedAt.Is(1499827319579);
    }

    /// <summary>
    /// An order with nothing filled reports an executed price of zero rather than dividing by it.
    /// </summary>
    /// <remarks>
    /// The executed price is a quotient guarded by a zero check, and every fixture here filled
    /// something - so only the dividing arm had ever run, in this converter and in two others beside
    /// it. Nothing filled is not an edge case: it is what a brand-new order looks like, which makes
    /// the unguarded version a crash on the commonest state an order has.
    /// </remarks>
    [Fact]
    public void NothingFilled_IsAZeroPriceAndNotADivision()
    {
        // arrange
        var raw = Raw.Replace(@"""executedQty"": ""6.4""", @"""executedQty"": ""0""", StringComparison.Ordinal);

        // act
        var serializer = this.GetJsonSerializer(Constants.GetOrderKey);
        var deserialized = serializer.Deserialize<OrderModel>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        deserialized.ExecutedQty.Is(0m);
        deserialized.ExecutedPrice.Is(0m, "an unfilled order priced its fills");
    }
}
