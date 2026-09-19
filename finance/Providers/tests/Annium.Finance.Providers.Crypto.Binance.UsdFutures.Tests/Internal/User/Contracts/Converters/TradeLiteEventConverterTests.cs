using System.Text;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User.Contracts.Converters;

/// <summary>
/// Verifies that <c>TradeLiteEventConverter</c> reads the venue's earliest fill notice.
/// </summary>
/// <remarks>
/// The payload is a real message captured from the live stream on 2026-09-19. The venue documents it
/// nowhere this module could retrieve, and it is not a shape anyone would guess: the fill's own price and
/// quantity are <c>L</c> and <c>l</c>, while <c>p</c> and <c>q</c> beside them are the order's requested
/// values — zero on a market order. Reading the wrong pair yields a fill at a price of zero.
/// </remarks>
public class TradeLiteEventConverterTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TradeLiteEventConverterTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public TradeLiteEventConverterTests(ITestOutputHelper outputHelper)
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
    /// The fill is read from the fill's own fields, not from the order's requested ones.
    /// </summary>
    [Fact]
    public void Fill_IsReadFromTheFillsOwnFields()
    {
        // arrange - captured verbatim from the live user data stream
        var raw = """
            {"e":"TRADE_LITE","E":1789837689736,"T":1789837689736,"s":"DOTUSDT","q":"4.9","p":"0.000000",
             "m":false,"c":"6e8f8d89-5b04-4ede-88bb-52b7d8b2b0d2","S":"BUY","L":"1.135900","l":"4.9",
             "t":1190331610,"i":33878894217}
            """;

        // act
        var serializer = this.GetJsonSerializer(Constants.TradeLiteKey);
        var e = serializer.Deserialize<TradeLiteEvent?>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        e.Symbol.Is("DOTUSDT");
        e.OrderId.Is("33878894217");
        e.ClientOrderId.Is("6e8f8d89-5b04-4ede-88bb-52b7d8b2b0d2");
        e.TradeId.Is("1190331610");
        // L, not p: p is the order's requested price and is zero on a market order
        e.Price.Is(1.1359m, "the order's requested price was read instead of the fill's");
        e.Qty.Is(4.9m);
        e.IsMaker.IsFalse();
        e.At.Is(1789837689736L);
    }

    /// <summary>
    /// Any other event reads as nothing, so one stream dispatch can try each shape in turn.
    /// </summary>
    [Fact]
    public void AnotherEvent_IsNotRead()
    {
        // arrange
        var raw = """
            {"e":"ORDER_TRADE_UPDATE","T":1,"E":2,"o":{"s":"DOTUSDT","c":"x","i":1,"X":"NEW"}}
            """;

        // act
        var serializer = this.GetJsonSerializer(Constants.TradeLiteKey);
        var e = serializer.Deserialize<TradeLiteEvent?>(Encoding.UTF8.GetBytes(raw));

        // assert
        e.IsDefault("a different event was read as a fill notice");
    }
}
