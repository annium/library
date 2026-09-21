using System.Text;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Crypto.Binance.Base.Market.Contracts.Domain;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.Market.Contracts.Converters;

/// <summary>
/// Verifies that <c>InstrumentFiltersConverter</c> reads a symbol's <c>filters</c> array into an
/// <see cref="InstrumentFilters"/>, and in particular that this market type's one-inequality-per-side price
/// band is not widened into a two-sided one.
/// </summary>
/// <remarks>
/// Driven at the filters array rather than through a whole <c>/exchangeInfo</c> payload, so that a case costs
/// a dozen lines instead of a hundred.
/// </remarks>
public class InstrumentFiltersConverterTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstrumentFiltersConverterTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public InstrumentFiltersConverterTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the Binance USD-M futures provider so the converter under test is resolved from its actual
    /// registration.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceUsdFutures();
    }

    /// <summary>
    /// The published pair bounds a buy from above and a sell from below, and nothing else.
    /// </summary>
    /// <remarks>
    /// This is the case worth pinning, because the obvious reading of a pair of multipliers is a band that
    /// applies to both sides - and that reading is wrong in the expensive direction. Our own trading block
    /// rests buys at 0.7 of the market while the lower multiplier is 0.95, and the exchange has taken them on
    /// every run: a two-sided band would have this provider refuse orders that work.
    /// </remarks>
    [Fact]
    public void ThePriceBand_BoundsOneEndPerSide()
    {
        // arrange
        var raw =
            @"[
                {
                    ""filterType"": ""PRICE_FILTER"",
                    ""maxPrice"": ""4529764"",
                    ""minPrice"": ""556.80"",
                    ""tickSize"": ""0.10""
                },
                {
                    ""filterType"": ""LOT_SIZE"",
                    ""maxQty"": ""1000"",
                    ""minQty"": ""0.001"",
                    ""stepSize"": ""0.001""
                },
                {
                    ""filterType"": ""MARKET_LOT_SIZE"",
                    ""maxQty"": ""120"",
                    ""minQty"": ""0.001"",
                    ""stepSize"": ""0.001""
                },
                {
                    ""filterType"": ""MIN_NOTIONAL"",
                    ""notional"": ""5""
                },
                {
                    ""filterType"": ""PERCENT_PRICE"",
                    ""multiplierUp"": ""1.1500"",
                    ""multiplierDown"": ""0.8500"",
                    ""multiplierDecimal"": ""4""
                },
                {
                    ""filterType"": ""MAX_NUM_ORDERS"",
                    ""limit"": 200
                }
            ]";

        // act
        var filters = Deserialize(raw).NotNull();

        // assert - a buy has a ceiling and no floor, a sell has a floor and no ceiling
        filters.PercentPriceFilter.IsEqual(new PercentPriceFilter(decimal.Zero, 1.15m, 0.85m, decimal.Zero));
    }

    /// <summary>
    /// The rest of the filters are read as the instrument's bounds, with the two lot-size filters merged and
    /// the notional left unbounded above, which is what this market type publishes.
    /// </summary>
    [Fact]
    public void TheLotSizeFilters_AreMergedIntoTheirIntersection()
    {
        // arrange
        var raw =
            @"[
                {
                    ""filterType"": ""PRICE_FILTER"",
                    ""maxPrice"": ""4529764"",
                    ""minPrice"": ""556.80"",
                    ""tickSize"": ""0.10""
                },
                {
                    ""filterType"": ""LOT_SIZE"",
                    ""maxQty"": ""1000"",
                    ""minQty"": ""0.001"",
                    ""stepSize"": ""0.001""
                },
                {
                    ""filterType"": ""MARKET_LOT_SIZE"",
                    ""maxQty"": ""120"",
                    ""minQty"": ""0.002"",
                    ""stepSize"": ""0.010""
                },
                {
                    ""filterType"": ""MIN_NOTIONAL"",
                    ""notional"": ""5""
                },
                {
                    ""filterType"": ""PERCENT_PRICE"",
                    ""multiplierUp"": ""1.1500"",
                    ""multiplierDown"": ""0.8500"",
                    ""multiplierDecimal"": ""4""
                },
                {
                    ""filterType"": ""MAX_NUM_ORDERS"",
                    ""limit"": 200
                }
            ]";

        // act
        var filters = Deserialize(raw).NotNull();

        // assert
        filters.LotSizeFilter.IsEqual(new LotSizeFilter(0.002m, 120m, 0.010m));
        filters.PriceFilter.IsEqual(new PriceFilter(556.80m, 4529764m, 0.10m));
        filters.NotionalFilter.IsEqual(new NotionalFilter(5m, decimal.MaxValue));
        filters.MaxOrdersFilter.IsEqual(new MaxOrdersFilter(200));
    }

    /// <summary>
    /// A symbol published without the price band is kept, with the band reading as unbounded.
    /// </summary>
    /// <remarks>
    /// The band is the one filter whose absence is representable: no band is a statement, where a missing
    /// price or lot filter leaves nothing to compute an order from and drops the symbol.
    /// </remarks>
    [Fact]
    public void AMissingPriceBand_LeavesTheSymbolTradableAndUnbounded()
    {
        // arrange
        var raw =
            @"[
                {
                    ""filterType"": ""PRICE_FILTER"",
                    ""maxPrice"": ""4529764"",
                    ""minPrice"": ""556.80"",
                    ""tickSize"": ""0.10""
                },
                {
                    ""filterType"": ""LOT_SIZE"",
                    ""maxQty"": ""1000"",
                    ""minQty"": ""0.001"",
                    ""stepSize"": ""0.001""
                },
                {
                    ""filterType"": ""MARKET_LOT_SIZE"",
                    ""maxQty"": ""120"",
                    ""minQty"": ""0.001"",
                    ""stepSize"": ""0.001""
                },
                {
                    ""filterType"": ""MIN_NOTIONAL"",
                    ""notional"": ""5""
                },
                {
                    ""filterType"": ""MAX_NUM_ORDERS"",
                    ""limit"": 200
                }
            ]";

        // act
        var filters = Deserialize(raw).NotNull();

        // assert
        filters.PercentPriceFilter.IsEqual(
            new PercentPriceFilter(decimal.Zero, decimal.Zero, decimal.Zero, decimal.Zero)
        );
    }

    /// <summary>
    /// An array missing a filter an order cannot be built without reads as null, which drops the symbol.
    /// </summary>
    [Fact]
    public void AMissingPriceFilter_DropsTheFilters()
    {
        // arrange
        var raw =
            @"[
                {
                    ""filterType"": ""LOT_SIZE"",
                    ""maxQty"": ""1000"",
                    ""minQty"": ""0.001"",
                    ""stepSize"": ""0.001""
                },
                {
                    ""filterType"": ""MARKET_LOT_SIZE"",
                    ""maxQty"": ""120"",
                    ""minQty"": ""0.001"",
                    ""stepSize"": ""0.001""
                },
                {
                    ""filterType"": ""MIN_NOTIONAL"",
                    ""notional"": ""5""
                },
                {
                    ""filterType"": ""MAX_NUM_ORDERS"",
                    ""limit"": 200
                }
            ]";

        // act
        var filters = Deserialize(raw);

        // assert
        filters.IsDefault();
    }

    /// <summary>
    /// Deserializes a raw <c>filters</c> array through the provider's own registered serializer.
    /// </summary>
    /// <param name="raw">The raw JSON array.</param>
    /// <returns>The parsed filters, or null where the converter refuses the array.</returns>
    private InstrumentFilters? Deserialize(string raw)
    {
        var serializer = this.GetJsonSerializer(Constants.ExchangeInfoKey);

        return serializer.Deserialize<InstrumentFilters?>(Encoding.UTF8.GetBytes(raw));
    }
}
