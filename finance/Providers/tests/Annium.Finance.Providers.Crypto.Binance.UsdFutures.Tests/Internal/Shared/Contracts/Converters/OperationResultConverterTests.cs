using System.Text;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.Shared.Contracts.Converters;

/// <summary>
/// Verifies that <c>OperationResultConverter</c> reads Binance's <c>{code, msg}</c> error envelope into an
/// <see cref="OperationResult"/>, and that a payload without a <c>code</c> field deserializes to null instead
/// of throwing.
/// </summary>
public class OperationResultConverterTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResultConverterTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public OperationResultConverterTests(ITestOutputHelper outputHelper)
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
    /// A Binance error envelope is parsed into its numeric code and message.
    /// </summary>
    [Fact]
    public void Works()
    {
        // arrange
        var raw = @"{""code"":-1121,""msg"":""smth bad""}";

        // act - deserialize
        var serializer = this.GetJsonSerializer(Constants.InitOrderKey);
        var deserialized = serializer.Deserialize<OperationResult>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        deserialized.Code.Is(-1121);
        deserialized.Message.Is("smth bad");
    }

    /// <summary>
    /// A payload without a <c>code</c> field deserializes to null instead of throwing.
    /// </summary>
    [Fact]
    public void InvalidDataReturnsEmpty()
    {
        // arrange
        var raw = @"{""msg"":""smth bad""}";

        // act - deserialize
        var serializer = this.GetJsonSerializer(Constants.InitOrderKey);
        var deserialized = serializer.Deserialize<OperationResult>(Encoding.UTF8.GetBytes(raw));

        // assert - deserialization
        deserialized.IsDefault();
    }

    /// <summary>
    /// A <c>code</c> the exchange sends as a JSON string is read as the number it spells.
    /// </summary>
    /// <remarks>
    /// The algo endpoints answer a successful cancellation with
    /// <c>{"algoId":…,"clientAlgoId":…,"code":"200","msg":"success"}</c> - quoted, where every error
    /// payload sends a bare number. Reading only the number threw, and the throw surfaced as a parse
    /// failure on a response the exchange considered a success: a cancellation that had happened, reported
    /// as unreadable. Captured live on 2026-09-19.
    /// </remarks>
    [Fact]
    public void StringCodeIsRead()
    {
        // arrange - a real cancellation answer, verbatim
        var raw =
            @"{""algoId"":4000001910058351,""clientAlgoId"":""d0a897fb-5b04-4ac7-b678-2189ad628150"",""code"":""200"",""msg"":""success""}";

        // act
        var serializer = this.GetJsonSerializer(Constants.InitOrderKey);
        var deserialized = serializer.Deserialize<OperationResult>(Encoding.UTF8.GetBytes(raw)).NotNull();

        // assert
        deserialized.Code.Is(200);
        deserialized.Message.Is("success");
    }

    /// <summary>
    /// A <c>code</c> that is neither a number nor a numeric string leaves the result unread rather than
    /// throwing, so one malformed field cannot turn a whole response into an exception.
    /// </summary>
    [Fact]
    public void UnreadableCodeReturnsEmpty()
    {
        // arrange
        var raw = @"{""code"":""not a number"",""msg"":""smth bad""}";

        // act
        var serializer = this.GetJsonSerializer(Constants.InitOrderKey);
        var deserialized = serializer.Deserialize<OperationResult>(Encoding.UTF8.GetBytes(raw));

        // assert
        deserialized.IsDefault();
    }
}
