using System.Collections.Generic;
using System.Text;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User.Contracts.Converters;

/// <summary>
/// Verifies that <c>GetTradeResponseConverter</c> reads Binance's <c>GET /myTrades</c> response into a
/// <see cref="TradeModel"/>, including the <c>isMaker</c> flag and commission fields.
/// </summary>
public class GetTradeResponseConverterTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTradeResponseConverterTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public GetTradeResponseConverterTests(ITestOutputHelper outputHelper)
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
    /// A captured trade response is parsed into its ids, price/quantity and commission, and whether the
    /// account was the maker side.
    /// </summary>
    [Fact]
    public void Works()
    {
        // arrange
        var raw =
            @"{
            ""symbol"": ""BNBBTC"",
            ""id"": 28457,
            ""orderId"": 100234,
            ""orderListId"": -1,
            ""price"": ""4.00000100"",
            ""qty"": ""12.00070000"",
            ""quoteQty"": ""48.000012"",
            ""commission"": ""10.10000000"",
            ""commissionAsset"": ""BNB"",
            ""time"": 1499865549590,
            ""isBuyer"": true,
            ""isMaker"": true,
            ""isBestMatch"": true
        }";

        // act
        var serializer = this.GetJsonSerializer(Constants.GetTradeKey);
        var deserialized = serializer.Deserialize<TradeModel>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        deserialized.Id.Is("28457");
        deserialized.OrderId.Is("100234");
        deserialized.Symbol.Is("BNBBTC");
        deserialized.Price.Is(4.000001m);
        deserialized.Qty.Is(12.0007m);
        deserialized.CommissionAsset.Is("BNB");
        deserialized.CommissionAmount.Is(10.1m);
        deserialized.Maker.IsTrue();
        deserialized.Moment.Is(1499865549590);
    }

    /// <summary>
    /// A trade missing any of the three fields the domain needs is dropped rather than handed over.
    /// </summary>
    /// <remarks>
    /// Three independent conditions joined in one line, and nothing exercised any of them - so each
    /// case here omits one field and leaves the rest whole, which is what makes them independent.
    /// Omits rather than blanks: the rule is about a value that never arrived, and a zero is a value.
    /// A trade handed over without an order to attach it to, without an instrument, or without the
    /// asset its commission was taken in is one the next layer reads fields off and gets defaults.
    /// </remarks>
    /// <param name="missing">The field left out of the payload.</param>
    [Theory]
    [InlineData("orderId")]
    [InlineData("symbol")]
    [InlineData("commissionAsset")]
    public void SkipsATradeMissingWhatTheDomainNeeds(string missing)
    {
        // arrange - one field omitted, the rest of a real answer left intact
        var fields = new Dictionary<string, string>
        {
            ["symbol"] = @"""symbol"": ""BNBBTC""",
            ["orderId"] = @"""orderId"": 100234",
            ["commissionAsset"] = @"""commissionAsset"": ""BNB""",
        };
        fields.Remove(missing);

        var raw =
            $@"{{
            {string.Join(",\n            ", fields.Values)},
            ""id"": 28457,
            ""orderListId"": -1,
            ""price"": ""4.00000100"",
            ""qty"": ""12.00070000"",
            ""quoteQty"": ""48.000012"",
            ""commission"": ""10.10000000"",
            ""time"": 1499865549590,
            ""isBuyer"": true,
            ""isMaker"": true,
            ""isBestMatch"": true
        }}";

        // act
        var serializer = this.GetJsonSerializer(Constants.GetTradeKey);
        var deserialized = serializer.Deserialize<TradeModel?>(Encoding.UTF8.GetBytes(raw));

        // assert
        deserialized.IsDefault($"a trade with no {missing} was handed over");
    }
}
