using System.Text;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Crypto.Binance.Base.Market.Contracts.Domain;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.Market.Contracts.Market.Converters;

/// <summary>
/// Verifies that <c>InstrumentFiltersConverter</c> reads a symbol's <c>filters</c> array into an
/// <see cref="InstrumentFilters"/>: the two lot-size filters merged, the price band read per side and per end,
/// and an array missing a filter answered as the converter's contract says it is.
/// </summary>
/// <remarks>
/// Driven at the filters array rather than through a whole <c>/exchangeInfo</c> payload, so that a case costs
/// a dozen lines instead of a hundred - which is what makes it affordable to ask the questions a single
/// captured payload cannot: a band whose four ends are four different numbers, and an array without one.
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
    /// Registers the Binance Spot provider so the converter under test is resolved from its actual registration.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot();
    }

    /// <summary>
    /// Every end of the price band lands on the side it was published for.
    /// </summary>
    /// <remarks>
    /// The four multipliers are the ones the exchange publishes for the instrument the trading block uses, and
    /// they are four different numbers on purpose: a band read with the sides transposed, or with an end
    /// mirrored onto the other side, passes a fixture whose numbers repeat and fails this one.
    /// </remarks>
    [Fact]
    public void ThePriceBand_IsReadPerSideAndPerEnd()
    {
        // arrange
        var raw =
            @"[
              {
                ""filterType"": ""PRICE_FILTER"",
                ""minPrice"": ""0.00100000"",
                ""maxPrice"": ""10000.00000000"",
                ""tickSize"": ""0.00100000""
              },
              {
                ""filterType"": ""LOT_SIZE"",
                ""minQty"": ""0.01000000"",
                ""maxQty"": ""92233720.00000000"",
                ""stepSize"": ""0.01000000""
              },
              {
                ""filterType"": ""MARKET_LOT_SIZE"",
                ""minQty"": ""0.00000000"",
                ""maxQty"": ""184261.83000000"",
                ""stepSize"": ""0.00000000""
              },
              {
                ""filterType"": ""PERCENT_PRICE_BY_SIDE"",
                ""bidMultiplierUp"": ""1.2"",
                ""bidMultiplierDown"": ""0.5"",
                ""askMultiplierUp"": ""2"",
                ""askMultiplierDown"": ""0.8"",
                ""avgPriceMins"": 5
              },
              {
                ""filterType"": ""NOTIONAL"",
                ""minNotional"": ""5.00000000"",
                ""maxNotional"": ""9000000.00000000""
              },
              {
                ""filterType"": ""MAX_NUM_ORDERS"",
                ""maxNumOrders"": 200
              }
            ]";

        // act
        var filters = Deserialize(raw).NotNull();

        // assert
        filters.PercentPriceFilter.IsEqual(new PercentPriceFilter(0.5m, 1.2m, 0.8m, 2m));
    }

    /// <summary>
    /// The rest of the filters are read as the instrument's bounds, with the two lot-size filters merged.
    /// </summary>
    /// <remarks>
    /// The merge is the interesting half: an order must satisfy both the limit and the market lot filter, so
    /// the combination is the tighter minimum and maximum and the coarser step - not whichever filter the
    /// array happened to list last.
    /// </remarks>
    [Fact]
    public void TheLotSizeFilters_AreMergedIntoTheirIntersection()
    {
        // arrange
        var raw =
            @"[
              {
                ""filterType"": ""PRICE_FILTER"",
                ""minPrice"": ""0.00100000"",
                ""maxPrice"": ""10000.00000000"",
                ""tickSize"": ""0.00100000""
              },
              {
                ""filterType"": ""LOT_SIZE"",
                ""minQty"": ""0.01000000"",
                ""maxQty"": ""92233720.00000000"",
                ""stepSize"": ""0.01000000""
              },
              {
                ""filterType"": ""MARKET_LOT_SIZE"",
                ""minQty"": ""0.02000000"",
                ""maxQty"": ""184261.83000000"",
                ""stepSize"": ""0.00100000""
              },
              {
                ""filterType"": ""PERCENT_PRICE_BY_SIDE"",
                ""bidMultiplierUp"": ""1.2"",
                ""bidMultiplierDown"": ""0.5"",
                ""askMultiplierUp"": ""2"",
                ""askMultiplierDown"": ""0.8"",
                ""avgPriceMins"": 5
              },
              {
                ""filterType"": ""NOTIONAL"",
                ""minNotional"": ""5.00000000"",
                ""maxNotional"": ""9000000.00000000""
              },
              {
                ""filterType"": ""MAX_NUM_ORDERS"",
                ""maxNumOrders"": 200
              }
            ]";

        // act
        var filters = Deserialize(raw).NotNull();

        // assert
        filters.LotSizeFilter.IsEqual(new LotSizeFilter(0.02m, 184261.83m, 0.01m));
        filters.PriceFilter.IsEqual(new PriceFilter(0.001m, 10000m, 0.001m));
        filters.NotionalFilter.IsEqual(new NotionalFilter(5m, 9000000m));
        filters.MaxOrdersFilter.IsEqual(new MaxOrdersFilter(200));
    }

    /// <summary>
    /// A symbol published without the price band is kept, with the band reading as unbounded.
    /// </summary>
    /// <remarks>
    /// The band is the one filter whose absence is representable: no band is a statement, where a missing
    /// price or lot filter leaves nothing to compute an order from and drops the symbol. Every symbol open
    /// for trading carries one today, which is exactly why the behaviour is pinned rather than assumed - the
    /// day one does not, the instrument must still arrive.
    /// </remarks>
    [Fact]
    public void AMissingPriceBand_LeavesTheSymbolTradableAndUnbounded()
    {
        // arrange
        var raw =
            @"[
              {
                ""filterType"": ""PRICE_FILTER"",
                ""minPrice"": ""0.00100000"",
                ""maxPrice"": ""10000.00000000"",
                ""tickSize"": ""0.00100000""
              },
              {
                ""filterType"": ""LOT_SIZE"",
                ""minQty"": ""0.01000000"",
                ""maxQty"": ""92233720.00000000"",
                ""stepSize"": ""0.01000000""
              },
              {
                ""filterType"": ""MARKET_LOT_SIZE"",
                ""minQty"": ""0.00000000"",
                ""maxQty"": ""184261.83000000"",
                ""stepSize"": ""0.00000000""
              },
              {
                ""filterType"": ""NOTIONAL"",
                ""minNotional"": ""5.00000000"",
                ""maxNotional"": ""9000000.00000000""
              },
              {
                ""filterType"": ""MAX_NUM_ORDERS"",
                ""maxNumOrders"": 200
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
                ""minQty"": ""0.01000000"",
                ""maxQty"": ""92233720.00000000"",
                ""stepSize"": ""0.01000000""
              },
              {
                ""filterType"": ""MARKET_LOT_SIZE"",
                ""minQty"": ""0.00000000"",
                ""maxQty"": ""184261.83000000"",
                ""stepSize"": ""0.00000000""
              },
              {
                ""filterType"": ""NOTIONAL"",
                ""minNotional"": ""5.00000000"",
                ""maxNotional"": ""9000000.00000000""
              },
              {
                ""filterType"": ""MAX_NUM_ORDERS"",
                ""maxNumOrders"": 200
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
