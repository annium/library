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
/// Verifies that <c>AlgoUpdateEventConverter</c> reads the user data stream's <c>ALGO_UPDATE</c> event.
/// </summary>
/// <remarks>
/// Both payloads below were captured from the live stream on 2026-09-19 by placing and cancelling a real
/// conditional order. The exchange documents this event's payload only through a schema component that
/// cannot be fetched, so an invented fixture would assert what the author imagined - and this payload in
/// particular is not one anybody would imagine correctly: the event's order object is <c>o</c>, and the
/// order's type inside it is <c>o</c> as well.
/// </remarks>
public class AlgoUpdateEventConverterTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoUpdateEventConverterTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public AlgoUpdateEventConverterTests(ITestOutputHelper outputHelper)
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
    /// A placement event is read in full, with the order type taken from the inner <c>o</c> and not from the
    /// object that shares its name.
    /// </summary>
    [Fact]
    public void Placement_IsRead()
    {
        // arrange - captured verbatim from the live user data stream
        var raw = """
            {"e":"ALGO_UPDATE","T":1789835582916,"E":1789835582917,"o":{
              "caid":"8121aafc-cba3-4a01-a279-1dcb03a7b80f","aid":4000001910701524,"at":"CONDITIONAL",
              "o":"STOP_MARKET","s":"DOTUSDT","S":"BUY","ps":"BOTH","f":"GTC","q":"5","X":"NEW","ai":"",
              "tp":"1.1637","p":"0","V":"EXPIRE_MAKER","wt":"CONTRACT_PRICE","pm":"NONE","cp":false,
              "pP":false,"R":false,"tt":0,"gtd":0,"ia":false}}
            """;

        // act
        var serializer = this.GetJsonSerializer(Constants.AlgoUpdateKey);
        var e = serializer.Deserialize<AlgoUpdateEvent?>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        e.AlgoId.Is("4000001910701524");
        e.ClientAlgoId.Is("8121aafc-cba3-4a01-a279-1dcb03a7b80f");
        e.Symbol.Is("DOTUSDT");
        e.Side.Is(OrderSide.Buy);
        // the inner "o", which a reader matching on name alone would confuse with the object holding it
        e.Type.Is(OrderType.StopLossMarket);
        e.Range.Is(OrientationRange.Both);
        e.TotalQty.Is(5m);
        e.LevelPrice.Is(1.1637m);
        e.Price.Is(0m);
        e.ReduceOnly.IsFalse();
        e.Status.Is(OrderStatus.New);
        e.IsSuperseded.IsFalse();
        e.UpdatedAt.Is(1789835582917L);
    }

    /// <summary>
    /// A cancellation event is read as terminal.
    /// </summary>
    [Fact]
    public void Cancellation_IsTerminal()
    {
        // arrange - captured verbatim from the live user data stream
        var raw = """
            {"e":"ALGO_UPDATE","T":1789835583244,"E":1789835583244,"o":{
              "caid":"8121aafc-cba3-4a01-a279-1dcb03a7b80f","aid":4000001910701524,"at":"CONDITIONAL",
              "o":"STOP_MARKET","s":"DOTUSDT","S":"BUY","ps":"BOTH","f":"GTC","q":"5","X":"CANCELED","ai":"",
              "tp":"1.1637","p":"0","R":false,"tt":0,"gtd":0,"ia":false}}
            """;

        // act
        var serializer = this.GetJsonSerializer(Constants.AlgoUpdateKey);
        var e = serializer.Deserialize<AlgoUpdateEvent?>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        e.Status.Is(OrderStatus.Canceled);
        e.IsSuperseded.IsFalse();
    }

    /// <summary>
    /// A triggered event is flagged as superseded, because the ordinary order it became reports the outcome.
    /// </summary>
    /// <param name="algoStatus">A status an ordinary order supersedes.</param>
    [Theory]
    [InlineData("TRIGGERED")]
    [InlineData("FINISHED")]
    public void Triggered_IsSuperseded(string algoStatus)
    {
        // arrange
        var raw = $$$"""
            {"e":"ALGO_UPDATE","T":1,"E":2,"o":{
              "caid":"8121aafc-cba3-4a01-a279-1dcb03a7b80f","aid":1,"at":"CONDITIONAL",
              "o":"STOP_MARKET","s":"DOTUSDT","S":"BUY","ps":"BOTH","q":"5","X":"{{{algoStatus}}}",
              "ai":"33877541028","tp":"1.1","p":"0","R":false}}
            """;

        // act
        var serializer = this.GetJsonSerializer(Constants.AlgoUpdateKey);
        var e = serializer.Deserialize<AlgoUpdateEvent?>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        e.IsSuperseded.IsTrue(
            "a triggered conditional order was not flagged as superseded, so it would be kept alongside the order it became"
        );
    }

    /// <summary>
    /// Any other event reads as nothing, so one stream dispatch can try each shape in turn.
    /// </summary>
    [Fact]
    public void AnotherEvent_IsNotRead()
    {
        // arrange - an ORDER_TRADE_UPDATE, which has its own converter
        var raw = """
            {"e":"ORDER_TRADE_UPDATE","T":1,"E":2,"o":{"s":"DOTUSDT","c":"x","i":1,"X":"NEW"}}
            """;

        // act
        var serializer = this.GetJsonSerializer(Constants.AlgoUpdateKey);
        var e = serializer.Deserialize<AlgoUpdateEvent?>(Encoding.UTF8.GetBytes(raw));

        // assert
        e.IsDefault("a different event was read as a conditional order update");
    }
}
