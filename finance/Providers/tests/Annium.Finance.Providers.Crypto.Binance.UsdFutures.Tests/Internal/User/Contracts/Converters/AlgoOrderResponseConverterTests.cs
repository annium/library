using System.Text;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User.Contracts.Converters;

/// <summary>
/// Verifies that <c>AlgoOrderResponseConverter</c> reads a conditional order into an
/// <see cref="OrderModel"/> under the field names the algo endpoints actually use, and drops the records an
/// ordinary order already accounts for.
/// </summary>
/// <remarks>
/// Every payload below is a real answer from the live exchange, captured on 2026-09-19 and stored in
/// <c>finance/kb/providers/binance/2026.09/2026.09.19-algo-probe/</c>. The documentation publishes no
/// response schemas for this family, so an invented fixture would only assert what the author imagined.
/// </remarks>
public class AlgoOrderResponseConverterTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoOrderResponseConverterTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public AlgoOrderResponseConverterTests(ITestOutputHelper outputHelper)
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
    /// The placement answer is read under the algo field names, which are not the order endpoint's.
    /// </summary>
    [Fact]
    public void Placement_IsReadUnderTheAlgoFieldNames()
    {
        // arrange - POST /fapi/v1/algoOrder, verbatim
        var raw = """
            {
              "algoId": 4000001910058351,
              "clientAlgoId": "d0a897fb-5b04-4ac7-b678-2189ad628150",
              "algoType": "CONDITIONAL",
              "orderType": "STOP_MARKET",
              "symbol": "DOTUSDT",
              "side": "BUY",
              "positionSide": "BOTH",
              "timeInForce": "GTC",
              "quantity": "4.9",
              "algoStatus": "NEW",
              "triggerPrice": "1.183700",
              "price": "0.000000",
              "icebergQuantity": null,
              "selfTradePreventionMode": "EXPIRE_MAKER",
              "workingType": "CONTRACT_PRICE",
              "priceMatch": "NONE",
              "closePosition": false,
              "priceProtect": false,
              "reduceOnly": false,
              "createTime": 1789819033056,
              "updateTime": 1789819033056,
              "triggerTime": 0,
              "goodTillDate": 0
            }
            """;

        // act
        var serializer = this.GetJsonSerializer(Constants.AlgoOrderKey);
        var order = serializer.Deserialize<OrderModel?>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert - the id comes from algoId, not orderId; the client id from clientAlgoId
        order.Id.Is("4000001910058351");
        order.ClientOrderId.Is("d0a897fb-5b04-4ac7-b678-2189ad628150");
        order.Symbol.Is("DOTUSDT");
        order.Side.Is(OrderSide.Buy);
        // orderType, where the ordinary endpoint says type
        order.Type.Is(OrderType.StopLossMarket);
        order.Range.Is(OrientationRange.Both);
        // quantity, where the ordinary endpoint says origQty
        order.TotalQty.Is(4.9m);
        // triggerPrice, where the ordinary endpoint says stopPrice
        order.LevelPrice.Is(1.1837m);
        order.Price.Is(0m);
        order.ReduceOnly.IsFalse();
        // algoStatus, where the ordinary endpoint says status
        order.Status.Is(OrderStatus.New);
        order.ExecutedQty.Is(0m);
        order.CreatedAt.Is(1789819033056L);
        order.UpdatedAt.Is(1789819033056L);
    }

    /// <summary>
    /// A cancelled conditional order reads as canceled, carrying the three extra fields the history adds.
    /// </summary>
    [Fact]
    public void CancelledInHistory_IsTerminal()
    {
        // arrange - one element of GET /fapi/v1/allAlgoOrders, verbatim
        var raw = """
            {
              "algoId": 4000001910058351,
              "clientAlgoId": "d0a897fb-5b04-4ac7-b678-2189ad628150",
              "algoType": "CONDITIONAL",
              "orderType": "STOP_MARKET",
              "symbol": "DOTUSDT",
              "side": "BUY",
              "positionSide": "BOTH",
              "timeInForce": "GTC",
              "quantity": "4.9",
              "algoStatus": "CANCELED",
              "actualOrderId": "",
              "actualPrice": "0.0000000",
              "triggerPrice": "1.183700",
              "price": "0.000000",
              "icebergQuantity": null,
              "tpOrderType": "",
              "selfTradePreventionMode": "EXPIRE_MAKER",
              "workingType": "CONTRACT_PRICE",
              "priceMatch": "NONE",
              "closePosition": false,
              "priceProtect": false,
              "reduceOnly": false,
              "createTime": 1789819033056,
              "updateTime": 1789819033966,
              "triggerTime": 0,
              "goodTillDate": 0,
              "isActivated": false
            }
            """;

        // act
        var serializer = this.GetJsonSerializer(Constants.AlgoOrderKey);
        var order = serializer.Deserialize<OrderModel?>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        order.Status.Is(OrderStatus.Canceled);
        order.UpdatedAt.Is(1789819033966L);
    }

    /// <summary>
    /// A triggered conditional order is dropped: an ordinary order under the same client id already says
    /// what became of it, and says it better.
    /// </summary>
    /// <remarks>
    /// The payload is the record of an order that really did fire on 2026-09-19. Its
    /// <c>actualOrderId</c> names an ordinary order whose <c>clientOrderId</c> is the <c>clientAlgoId</c>
    /// below — which is why dropping this one loses nothing and keeps the caller from seeing one order
    /// twice. Mapping <c>FINISHED</c> to <see cref="OrderStatus.Filled"/> would read as correct here and be
    /// wrong whenever the book cancels the triggered order instead of filling it.
    /// </remarks>
    /// <param name="algoStatus">The status of a record an ordinary order supersedes.</param>
    [Theory]
    [InlineData("FINISHED")]
    [InlineData("TRIGGERED")]
    public void Triggered_IsDropped(string algoStatus)
    {
        // arrange - GET /fapi/v1/allAlgoOrders after a real trigger, verbatim but for the status
        var raw = $$"""
            {
              "algoId": 4000001910368571,
              "clientAlgoId": "60d0f110-7316-4073-a41f-5c5c699a83ed",
              "algoType": "CONDITIONAL",
              "orderType": "STOP_MARKET",
              "symbol": "DOTUSDT",
              "side": "BUY",
              "positionSide": "BOTH",
              "timeInForce": "GTC",
              "quantity": "4.9",
              "algoStatus": "{{algoStatus}}",
              "actualOrderId": "33877541028",
              "actualPrice": "1.1325000",
              "actualQty": "4.9",
              "actualType": "MARKET",
              "triggerPrice": "1.132200",
              "price": "0.000000",
              "createTime": 1789826233630,
              "updateTime": 1789826323611,
              "triggerTime": 1789826323607,
              "isActivated": false
            }
            """;

        // act
        var serializer = this.GetJsonSerializer(Constants.AlgoOrderKey);
        var order = serializer.Deserialize<OrderModel?>(Encoding.UTF8.GetBytes(raw));

        // assert
        order.IsDefault("a triggered conditional order reached the caller, duplicating the order it became");
    }

    /// <summary>
    /// A status this venue has not shown us is dropped rather than guessed at.
    /// </summary>
    /// <remarks>
    /// The opposite of the ordinary order mapping, which defaults an unknown status to the enum's first
    /// member. Here an unrecognized value means the exchange added a state to a family we have just
    /// migrated onto, and inventing a status for it would put a wrong order in front of a caller rather
    /// than nothing at all.
    /// </remarks>
    [Fact]
    public void UnknownStatus_IsDropped()
    {
        // arrange
        var raw = """
            {
              "algoId": 1,
              "clientAlgoId": "d0a897fb-5b04-4ac7-b678-2189ad628150",
              "orderType": "STOP_MARKET",
              "symbol": "DOTUSDT",
              "side": "BUY",
              "quantity": "4.9",
              "algoStatus": "SOMETHING_NEW",
              "triggerPrice": "1.1",
              "createTime": 1,
              "updateTime": 2
            }
            """;

        // act
        var serializer = this.GetJsonSerializer(Constants.AlgoOrderKey);
        var order = serializer.Deserialize<OrderModel?>(Encoding.UTF8.GetBytes(raw));

        // assert
        order.IsDefault("an unrecognized algo status was given a domain meaning it does not have");
    }

    /// <summary>
    /// A record with no client id is dropped: the client id is the identity this module keys orders by.
    /// </summary>
    [Fact]
    public void WithoutClientAlgoId_IsDropped()
    {
        // arrange
        var raw = """
            {
              "algoId": 1,
              "orderType": "STOP_MARKET",
              "symbol": "DOTUSDT",
              "side": "BUY",
              "quantity": "4.9",
              "algoStatus": "NEW",
              "triggerPrice": "1.1",
              "createTime": 1,
              "updateTime": 2
            }
            """;

        // act
        var serializer = this.GetJsonSerializer(Constants.AlgoOrderKey);
        var order = serializer.Deserialize<OrderModel?>(Encoding.UTF8.GetBytes(raw));

        // assert
        order.IsDefault("an order with no client id was accepted, and this module has no other identity for it");
    }
}
