using System;
using System.Text;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Contracts.Domain;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User.Contracts.Converters;

/// <summary>
/// Verifies that <c>CancelOrderResponseConverter</c> reads Binance's <c>DELETE /order</c> response into a
/// <see cref="CancelOrderResponse"/>, notably parsing the returned <c>clientOrderId</c> - which Binance
/// generates fresh for the cancel confirmation - as a <see cref="Guid"/>.
/// </summary>
public class CancelOrderResponseConverterTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelOrderResponseConverterTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public CancelOrderResponseConverterTests(ITestOutputHelper outputHelper)
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
    /// A captured cancel-order response is parsed into the numeric order id and the client order id, the
    /// latter carried as a GUID.
    /// </summary>
    [Fact]
    public void Success()
    {
        // arrange
        var raw =
            @"{
            ""symbol"": ""LTCBTC"",
            ""origClientOrderId"": ""myOrder1"",
            ""orderId"": 4,
            ""orderListId"": -1,
            ""clientOrderId"": ""c5a7733c-ad87-4c6d-a2a4-941ee8e0f7f7"",
            ""transactTime"": 1684804350068,
            ""price"": ""2.00000000"",
            ""origQty"": ""1.00000000"",
            ""executedQty"": ""0.00000000"",
            ""cummulativeQuoteQty"": ""0.00000000"",
            ""status"": ""CANCELED"",
            ""timeInForce"": ""GTC"",
            ""type"": ""LIMIT"",
            ""side"": ""BUY"",
            ""selfTradePreventionMode"": ""NONE""
        }";

        // act
        var serializer = this.GetJsonSerializer(Constants.CancelOrderKey);
        var deserialized = serializer.Deserialize<CancelOrderResponse>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        deserialized.OrderId.Is("4");
        deserialized.Id.Is(Guid.Parse("c5a7733c-ad87-4c6d-a2a4-941ee8e0f7f7"));
    }

    /// <summary>
    /// A client order id the exchange did not get from us is not a GUID, and the whole response is dropped.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The converter reads <c>clientOrderId</c> as a GUID and returns <c>default</c> when it is not one, so
    /// the caller is told the cancel produced nothing rather than that it produced an order it cannot name.
    /// </para>
    /// <para>
    /// This is not a hypothetical shape. Binance assigns its own ids to anything placed outside this
    /// client — the web interface uses <c>web_…</c>, liquidations and auto-closes get <c>autoclose-…</c> —
    /// and every one of them lands here. Cancelling such an order succeeds at the exchange and reads as
    /// nothing at all on this side. Pinned because it is a deliberate narrowing and reads like an oversight:
    /// the fix, whenever it comes, should be a decision about what to return, not a surprise.
    /// </para>
    /// </remarks>
    /// <param name="clientOrderId">A client order id Binance really does assign.</param>
    [Theory]
    [InlineData("web_kR8Xq2v1")]
    [InlineData("autoclose-1700000000000")]
    [InlineData("")]
    public void ClientOrderIdThatIsNotAGuid_DropsTheResponse(string clientOrderId)
    {
        // arrange
        var raw =
            $@"{{
            ""clientOrderId"": ""{clientOrderId}"",
            ""orderId"": 283194212,
            ""status"": ""CANCELED"",
            ""symbol"": ""BTCUSDT""
        }}";

        // act
        var serializer = this.GetJsonSerializer(Constants.CancelOrderKey);
        var deserialized = serializer.Deserialize<CancelOrderResponse>(Encoding.UTF8.GetBytes(raw));

        // assert
        deserialized.IsDefault($"'{clientOrderId}' produced a response instead of being dropped");
    }
}
