using System;
using System.Net.Mime;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;
using static Annium.Finance.Providers.Crypto.Binance.UsdFutures.Constants;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests;

/// <summary>
/// Pins that every key this venue registers under can be resolved under.
/// </summary>
/// <remarks>
/// A registration and a resolution name the same key twice, in two files, and nothing connects them but the
/// constant. Get one wrong and nothing complains until a connector is built against a real account - which
/// is the most expensive place to find out. The list below is the venue's registration read back.
/// </remarks>
public class ProviderRegistrationTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderRegistrationTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public ProviderRegistrationTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the USD-M futures provider.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceUsdFutures();
    }

    /// <summary>
    /// Every key registered with an HTTP request factory resolves one.
    /// </summary>
    [Fact]
    public void EveryRequestFactoryKeyResolves()
    {
        // arrange
        var sp = Get<IServiceProvider>();
        string[] keys =
        [
            ExchangeInfoKey,
            CandleKey,
            InstrumentTickerKey,
            ServerTimeKey,
            GetAccountKey,
            GetOrderKey,
            GetTradeKey,
            SetLeverageKey,
            InitOrderKey,
            ModifyOrderKey,
            CancelOrderKey,
            CancelAllOrdersKey,
            ListenKeyKey,
        ];

        // act
        // assert
        foreach (var key in keys)
            sp.ResolveHttpRequestFactory(key).IsNotDefault(key);
    }

    /// <summary>
    /// Every key registered with a serializer resolves one.
    /// </summary>
    /// <remarks>
    /// The three stream payload keys have no request factory of their own - nothing fetches them over HTTP -
    /// so they are the ones a resolution-by-request-factory sweep would miss entirely.
    /// </remarks>
    [Fact]
    public void EverySerializerKeyResolves()
    {
        // arrange
        var sp = Get<IServiceProvider>();
        string[] keys =
        [
            ExchangeInfoKey,
            CandleKey,
            InstrumentTickerKey,
            ServerTimeKey,
            GetAccountKey,
            GetOrderKey,
            GetTradeKey,
            SetLeverageKey,
            InitOrderKey,
            ModifyOrderKey,
            CancelOrderKey,
            CancelAllOrdersKey,
            ListenKeyKey,
            AccountConfigurationUpdateKey,
            BalanceAndPositionUpdateKey,
            OrderUpdateKey,
        ];

        // act
        // assert
        foreach (var key in keys)
            sp.ResolveSerializer<ReadOnlyMemory<byte>>(key, MediaTypeNames.Application.Json).IsNotDefault(key);
    }
}
