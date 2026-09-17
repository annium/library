using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Net.Http;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Tests.Shared.HttpExtensions;

/// <summary>
/// Verifies that a signed request is signed over exactly the query string it sends.
/// </summary>
/// <remarks>
/// <para>
/// Binance checks the signature against the query string it receives, so the only thing that makes a
/// signature valid is that the two are the same bytes. They are not obviously the same: parameters are
/// given as raw values and the query is percent-encoded on its way into the URI, so a signer that built its
/// own string from the raw values would sign something the exchange never sees — and every signed endpoint
/// would answer <c>-1022 Signature for this request is not valid</c>, from call sites that read as correct.
/// </para>
/// <para>
/// What keeps the two the same is that <c>Signature</c> signs <c>req.Uri.Query</c> — the composed query,
/// not a reconstruction of it. That is a property of the implementation rather than of the contract, which
/// is why it is pinned here: the alternative is a one-line change that looks harmless and breaks
/// everything signed at once.
/// </para>
/// <para>
/// These assert against what <b>arrived at a server</b> rather than what the request object holds, because
/// the question is about the bytes on the wire and nothing short of sending them answers it. Signing does
/// not even happen until then: <c>Signature</c> registers a <c>Configure</c> action, so a test that built a
/// request and inspected it would find the signer had never been called.
/// </para>
/// <para>
/// The signature test that runs against the exchange checks something narrower — that our HMAC of a fixed
/// string matches a value Binance produced. It cannot see this property at all: it hands the signer a
/// literal, so no query is composed and nothing is encoded.
/// </para>
/// </remarks>
public class HttpRequestSignatureExtensionsTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequestSignatureExtensionsTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public HttpRequestSignatureExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        this.RegisterHttpRequestFactory();
    }

    /// <summary>
    /// Every parameter shape percent-encoding changes: the string signed is the string that arrives.
    /// </summary>
    /// <param name="value">A parameter value that does not survive a query unencoded.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("LTCBTC")]
    [InlineData("a b")]
    [InlineData("a+b")]
    [InlineData("a&b")]
    [InlineData("a=b")]
    [InlineData("[{\"a\":1}]")]
    [InlineData("привет")]
    public async Task SignedQuery_IsTheQueryThatArrives(string value)
    {
        // arrange
        var signer = new RecordingSignatureService();
        var received = string.Empty;
        await using var server = this.RunHttpServer(
            (request, response) =>
            {
                received = request.Url?.Query.TrimStart('?') ?? string.Empty;
                response.Ok();

                return Task.CompletedTask;
            }
        );

        // act
        await this.CreateHttpRequest(server)
            .Get("order")
            .Param("symbol", "LTCBTC")
            .Param("newClientOrderId", value)
            .Sign(signer)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert - the signer was asked exactly once, for the query that arrived, minus the signature it
        // produced: appending that afterwards is what lets the two agree at all
        signer.Signed.Has(1);
        var withoutSignature = string.Join(
            '&',
            received.Split('&').Where(x => !x.StartsWith("signature=", StringComparison.Ordinal))
        );
        signer.Signed.At(0).Is(withoutSignature, "signed a different string than the one that was sent");

        // assert - and what arrived really carries the signature the signer returned
        received.Contains($"signature={RecordingSignatureService.Signature}").IsTrue();
    }

    /// <summary>
    /// The value arrives percent-encoded — the half the assertion above cannot see on its own, since a
    /// request that encoded nothing would still have the two strings agree with each other, and with
    /// nothing Binance accepts.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SignedQuery_IsPercentEncoded()
    {
        // arrange
        var signer = new RecordingSignatureService();
        await using var server = this.RunHttpServer(
            (_, response) =>
            {
                response.Ok();

                return Task.CompletedTask;
            }
        );

        // act
        await this.CreateHttpRequest(server)
            .Get("order")
            .Param("newClientOrderId", "a b&c=d")
            .Sign(signer)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        signer.Signed.Has(1);
        signer.Signed.At(0).StartsWith("newClientOrderId=a%20b%26c%3Dd").IsTrue();
    }

    /// <summary>
    /// The timestamp comes from the signature service's synced server time, not from this machine's clock.
    /// </summary>
    /// <remarks>
    /// Binance rejects a request whose timestamp is outside <c>recvWindow</c> of its own clock, so signing
    /// with local time works exactly as long as the two agree and fails as soon as they drift — the kind of
    /// failure that arrives on someone else's machine, intermittently, as <c>-1021</c>. The fake reports a
    /// 2017 timestamp, so a value taken from the clock is not a near miss here; it is nine years out.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Timestamp_ComesFromTheServerTimeSource()
    {
        // arrange
        var signer = new RecordingSignatureService();
        var received = string.Empty;
        await using var server = this.RunHttpServer(
            (request, response) =>
            {
                received = request.Url?.Query.TrimStart('?') ?? string.Empty;
                response.Ok();

                return Task.CompletedTask;
            }
        );

        // act
        await this.CreateHttpRequest(server).Get("order").Sign(signer).RunAsync(TestContext.Current.CancellationToken);

        // assert
        received
            .Contains($"timestamp={RecordingSignatureService.ServerTimeValue}")
            .IsTrue($"timestamp did not come from the server time source: {received}");
    }

    /// <summary>
    /// The receive window is the documented 30 seconds, and it is sent in milliseconds.
    /// </summary>
    /// <remarks>
    /// The unit is the whole risk: Binance reads this as milliseconds and caps it at 60000, so a value
    /// meant as seconds is a window of 30 milliseconds — every request rejected as stale — and one meant
    /// as minutes exceeds the cap outright.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ReceiveWindow_IsThirtySecondsInMilliseconds()
    {
        // arrange
        var received = string.Empty;
        await using var server = this.RunHttpServer(
            (request, response) =>
            {
                received = request.Url?.Query.TrimStart('?') ?? string.Empty;
                response.Ok();

                return Task.CompletedTask;
            }
        );

        // act
        await this.CreateHttpRequest(server)
            .Get("order")
            .ReceiveWindow()
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        received.Contains("recvWindow=30000").IsTrue($"unexpected receive window: {received}");
    }

    /// <summary>
    /// A signature service that records what it was asked to sign and answers with a fixed value.
    /// </summary>
    private sealed class RecordingSignatureService : ISignatureService
    {
        /// <summary>The value this signer always returns, so the request's own signature is recognisable.</summary>
        public const string Signature = "0123456789abcdef";

        /// <summary>Gets the strings this signer was asked to sign, in call order.</summary>
        public IReadOnlyList<string> Signed => _signed;

        /// <summary>The fixed server time this signer reports — 2017, so a value read off the clock is unmistakable.</summary>
        public const long ServerTimeValue = 1_499_827_319_559;

        /// <summary>Gets a fixed server time, so the timestamp the signer adds does not vary between runs.</summary>
        public long ServerTime => ServerTimeValue;

        /// <summary>The recorded strings.</summary>
        private readonly List<string> _signed = new();

        /// <summary>Gets a fixed API key.</summary>
        /// <returns>The key.</returns>
        public string GetKey() => "test-key";

        /// <summary>Records the string and returns the fixed signature.</summary>
        /// <param name="data">The string to sign.</param>
        /// <returns>The fixed signature.</returns>
        public string GetSignature(string data)
        {
            _signed.Add(data);

            return Signature;
        }
    }
}
