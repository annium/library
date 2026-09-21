using System.Text;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Crypto.Binance.Base.Market.Contracts.Domain;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.Market.Contracts.Domain;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.Market.Contracts.Converters;

/// <summary>
/// Verifies that <c>ExchangeInfoConverter</c> reads Binance's <c>/exchangeInfo</c> response - the account-wide
/// weight rate limit and the per-symbol trading rules - into a <see cref="ExchangeInfo"/>, collapsing the
/// scattered price/quantity/notional filters onto a single <see cref="InstrumentModel"/>, and that a payload
/// missing the fields it needs deserializes to null instead of throwing.
/// </summary>
public class ExchangeInfoConverterTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExchangeInfoConverterTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public ExchangeInfoConverterTests(ITestOutputHelper outputHelper)
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
    /// A captured <c>/exchangeInfo</c> response is parsed into the request-weight rate limit and, for the one
    /// symbol it carries, the price/quantity/notional bounds and max open order count pulled out of its filters.
    /// </summary>
    [Fact]
    public void Works()
    {
        // arrange
        var raw =
            @"{
            ""timezone"": ""UTC"",
            ""serverTime"": 1690214411331,
            ""futuresType"": ""U_MARGINED"",
            ""rateLimits"": [
                {
                    ""rateLimitType"": ""REQUEST_WEIGHT"",
                    ""interval"": ""MINUTE"",
                    ""intervalNum"": 1,
                    ""limit"": 2400
                },
                {
                    ""rateLimitType"": ""ORDERS"",
                    ""interval"": ""MINUTE"",
                    ""intervalNum"": 1,
                    ""limit"": 1200
                },
                {
                    ""rateLimitType"": ""ORDERS"",
                    ""interval"": ""SECOND"",
                    ""intervalNum"": 10,
                    ""limit"": 300
                }
            ],
            ""exchangeFilters"": [],
            ""assets"": [
                {
                    ""asset"": ""USDT"",
                    ""marginAvailable"": true,
                    ""autoAssetExchange"": ""-10000""
                },
                {
                  ""asset"": ""ETH"",
                  ""marginAvailable"": true,
                  ""autoAssetExchange"": ""-5""
                }
            ],
            ""symbols"": [
                {
                    ""symbol"": ""BTCUSDT"",
                    ""pair"": ""BTCUSDT"",
                    ""contractType"": ""PERPETUAL"",
                    ""deliveryDate"": 4133404800000,
                    ""onboardDate"": 1569398400000,
                    ""status"": ""TRADING"",
                    ""maintMarginPercent"": ""2.5000"",
                    ""requiredMarginPercent"": ""5.0000"",
                    ""baseAsset"": ""BTC"",
                    ""quoteAsset"": ""USDT"",
                    ""marginAsset"": ""USDT"",
                    ""pricePrecision"": 2,
                    ""quantityPrecision"": 3,
                    ""baseAssetPrecision"": 8,
                    ""quotePrecision"": 8,
                    ""underlyingType"": ""COIN"",
                    ""underlyingSubType"": [
                        ""PoW""
                    ],
                    ""settlePlan"": 0,
                    ""triggerProtect"": ""0.0500"",
                    ""liquidationFee"": ""0.012500"",
                    ""marketTakeBound"": ""0.05"",
                    ""maxMoveOrderLimit"": 10000,
                    ""filters"": [
                        {
                            ""minPrice"": ""556.80"",
                            ""maxPrice"": ""4529764"",
                            ""filterType"": ""PRICE_FILTER"",
                            ""tickSize"": ""0.10""
                        },
                        {
                            ""stepSize"": ""0.001"",
                            ""filterType"": ""LOT_SIZE"",
                            ""maxQty"": ""1000"",
                            ""minQty"": ""0.001""
                        },
                        {
                            ""stepSize"": ""0.001"",
                            ""filterType"": ""MARKET_LOT_SIZE"",
                            ""maxQty"": ""120"",
                            ""minQty"": ""0.001""
                        },
                        {
                            ""limit"": 200,
                            ""filterType"": ""MAX_NUM_ORDERS""
                        },
                        {
                            ""limit"": 10,
                            ""filterType"": ""MAX_NUM_ALGO_ORDERS""
                        },
                        {
                            ""notional"": ""5.0"",
                            ""filterType"": ""MIN_NOTIONAL""
                        },
                        {
                            ""multiplierDown"": ""0.9500"",
                            ""multiplierUp"": ""1.0500"",
                            ""multiplierDecimal"": ""4"",
                            ""filterType"": ""PERCENT_PRICE""
                        }
                    ],
                    ""orderTypes"": [
                        ""LIMIT"",
                        ""MARKET"",
                        ""STOP"",
                        ""STOP_MARKET"",
                        ""TAKE_PROFIT"",
                        ""TAKE_PROFIT_MARKET"",
                        ""TRAILING_STOP_MARKET""
                    ],
                    ""timeInForce"": [
                        ""GTC"",
                        ""IOC"",
                        ""FOK"",
                        ""GTX""
                    ]
                }
            ]
        }";

        // act - deserialize
        var serializer = this.GetJsonSerializer(Constants.ExchangeInfoKey);
        var deserialized = serializer.Deserialize<ExchangeInfo?>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert - deserialization
        deserialized.RateLimits.IsEqual(new RateLimits(2400));
        deserialized.Assets.Has(2);
        deserialized.Assets.At(0).Code.Is("USDT");
        deserialized.Assets.At(1).Code.Is("ETH");
        deserialized.Instruments.Has(1);
        var btcusdt = deserialized.Instruments.At(0);
        btcusdt.IsEqual(
            new InstrumentModel(
                "BTCUSDT",
                new ResourceModel("BTC", 8),
                new ResourceModel("USDT", 8),
                new ResourceModel("USDT", 8),
                0.001m,
                120m,
                0.001m,
                556.8m,
                4529764m,
                0.1m,
                decimal.Zero,
                1.05m,
                0.95m,
                decimal.Zero,
                5m,
                decimal.MaxValue,
                200
            )
        );
    }

    /// <summary>
    /// An asset the exchange will not take as margin is dropped, rather than offered as one that works.
    /// </summary>
    /// <remarks>
    /// The rule is a filter, and a filter is only half-tested by a payload where everything passes: the
    /// recorded fixture carries two assets and both are marginable, so nothing distinguished this
    /// converter from one that returns every asset it reads. Offering a non-marginable asset is not a
    /// display defect - it is an order sized against collateral the exchange will refuse.
    /// </remarks>
    [Fact]
    public void AssetThatIsNotMarginable_IsDropped()
    {
        // arrange
        var raw = Payload(
            """
                {
                    "asset": "USDT",
                    "marginAvailable": true,
                    "autoAssetExchange": "-10000"
                },
                {
                    "asset": "SHIB",
                    "marginAvailable": false,
                    "autoAssetExchange": "0"
                }
            """
        );

        // act
        var serializer = this.GetJsonSerializer(Constants.ExchangeInfoKey);
        var deserialized = serializer.Deserialize<ExchangeInfo?>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        deserialized.Assets.Has(1);
        deserialized.Assets.At(0).Code.Is("USDT");
    }

    /// <summary>
    /// Exchange info with no <c>REQUEST_WEIGHT</c> limit is dropped whole, rather than read with no ceiling.
    /// </summary>
    /// <remarks>
    /// The rate limiter's ceiling comes from this entry. Reading the payload without it would leave the
    /// limiter on whatever it was built with while the exchange enforces something else - the failure
    /// arriving later, as a ban, from code that looks like it is respecting a limit.
    /// </remarks>
    [Fact]
    public void ExchangeInfoWithoutRequestWeightLimit_IsDropped()
    {
        // arrange - every limit the exchange sends except the one that is read
        var raw = Payload(
            """
                {
                    "asset": "USDT",
                    "marginAvailable": true,
                    "autoAssetExchange": "-10000"
                }
            """,
            """
                {
                    "rateLimitType": "ORDERS",
                    "interval": "MINUTE",
                    "intervalNum": 1,
                    "limit": 1200
                }
            """
        );

        // act
        var serializer = this.GetJsonSerializer(Constants.ExchangeInfoKey);
        var deserialized = serializer.Deserialize<ExchangeInfo?>(Encoding.UTF8.GetBytes(raw));

        // assert
        deserialized.IsDefault();
    }

    /// <summary>
    /// A payload missing the fields the converter needs deserializes to null instead of throwing.
    /// </summary>
    [Fact]
    public void SkipsInvalidData()
    {
        // arrange
        var raw =
            @"{
            ""s"": """"
        }";

        // act - deserialize
        var serializer = this.GetJsonSerializer(Constants.ExchangeInfoKey);
        var deserialized = serializer.Deserialize<ExchangeInfo?>(Encoding.UTF8.GetBytes(raw));

        // assert - deserialization
        deserialized.IsDefault();
    }

    /// <summary>
    /// Builds an exchange-info payload around the parts a test varies.
    /// </summary>
    /// <remarks>
    /// The instrument list is left empty on purpose. These tests are about the asset and rate-limit
    /// blocks, and an empty <c>symbols</c> array deserializes to an empty collection rather than to
    /// nothing - so carrying a second copy of the recorded instrument would add a fixture to keep in
    /// step with the exchange in exchange for nothing either test asserts.
    /// </remarks>
    /// <param name="assets">The <c>assets</c> entries, as JSON.</param>
    /// <param name="rateLimits">The <c>rateLimits</c> entries; the documented REQUEST_WEIGHT when omitted.</param>
    /// <returns>The payload.</returns>
    private static string Payload(string assets, string? rateLimits = null) =>
        $$"""
        {
            "timezone": "UTC",
            "serverTime": 1690214411331,
            "futuresType": "U_MARGINED",
            "rateLimits": [
        {{rateLimits
            ?? """
                {
                    "rateLimitType": "REQUEST_WEIGHT",
                    "interval": "MINUTE",
                    "intervalNum": 1,
                    "limit": 2400
                }
            """}}
            ],
            "exchangeFilters": [],
            "assets": [
        {{assets}}
            ],
            "symbols": []
        }
        """;
}
