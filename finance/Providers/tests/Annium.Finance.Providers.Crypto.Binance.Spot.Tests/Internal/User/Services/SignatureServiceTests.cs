using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User.Services;

/// <summary>
/// Verifies that our HMAC-SHA256 of a fixed string under the account's secret equals a value Binance
/// produced for the same input, pinned in <c>test.env</c> rather than checked against a live account.
/// </summary>
/// <remarks>
/// <para>
/// This is a conformance check on the hash and nothing more: same algorithm, same key material, same bytes
/// in, same digest out. It is worth having — a signature that is subtly wrong fails every signed endpoint
/// with one opaque code — but it is narrow, and it used to be described as if it were broader.
/// </para>
/// <para>
/// In particular it says nothing about percent-encoding, and cannot: it hands the signer a literal, so no
/// query is composed and nothing is encoded on the way. That property — that the string signed is the
/// string sent — lives one level up, at <c>Signature</c>, and is pinned offline by
/// <c>HttpRequestSignatureExtensionsTests</c>, which sends real requests to a local server and compares
/// what the signer was asked for against what arrived.
/// </para>
/// </remarks>
[Collection(ExchangeCollection.Name)]
[Trait(TestBlock.Name, TestBlock.Read)]
public class SignatureServiceTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignatureServiceTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public SignatureServiceTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the Binance Spot provider so the signature service under test is resolved from its actual registration.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot();
    }

    /// <summary>
    /// Signs a fixed order query string with the credentials from <c>test.env</c> and asserts it matches the
    /// signature pinned in <see cref="Settings.ExpectedSignature"/>.
    /// </summary>
    // No Timeout: this test signs a string and compares it. Nothing in its body waits, so a deadline
    // here would guard nothing - and xUnit1069 says as much, since there is no token to observe.
    // What can hang on this path is the fixture around it, and that is bounded where it is built.
    [Fact(
        Skip = "needs exchange credentials in test.env",
        SkipUnless = nameof(Exchange.HasCredentials),
        SkipType = typeof(Exchange)
    )]
    public void Signature_IsValid()
    {
        // arrange
        var settings = Settings.User;
        var providerKey = settings.GetProviderKey();
        var service = Provider.CreateSignatureService(settings, providerKey);
        var expectedSignature = Settings.ExpectedSignature;
        var query =
            "symbol=LTCBTC&side=BUY&type=LIMIT&timeInForce=GTC&quantity=1&price=0.1&recvWindow=5000&timestamp=1499827319559";

        // act
        var signature = service.GetSignature(query);

        // assert
        service.GetKey().Is(Settings.User.Key);
        signature.Is(expectedSignature);
    }
}
