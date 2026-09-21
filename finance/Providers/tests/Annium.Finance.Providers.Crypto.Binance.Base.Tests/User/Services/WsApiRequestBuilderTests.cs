using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Tests.User.Services;

/// <summary>
/// Pins how a WebSocket API request is composed and signed, against the worked examples the venue publishes.
/// </summary>
/// <remarks>
/// <para>
/// The examples are the reason these tests are worth more than their length suggests. A signature is the one
/// thing in this codebase with no partial failure: it is right or the request is refused, and the refusal
/// names no parameter, no ordering and no encoding. Checking our arithmetic against a value the venue
/// published removes every one of those from the list of things a live failure could mean.
/// </para>
/// <para>
/// The key and secret below are the venue's own illustrative pair, printed in its documentation beside the
/// expected signatures. They authorize nothing and are the only values that reproduce those signatures.
/// </para>
/// </remarks>
public class WsApiRequestBuilderTests : ProvidersTestBase
{
    /// <summary>The documented example API key.</summary>
    private const string DocKey = "vmPUZE6mv9SD5VNHk4HlWFsOr6aKE2zvsw0MuIgwCIPy6utIco14y7Ju91duEh8A";

    /// <summary>The documented example API secret.</summary>
    private const string DocSecret = "NhqPtmdSJYdKjVHjA7PZj4Mge3R5YNiP1e3UZjInClVN65XAbvqqM6A7H5fATj0j";

    /// <summary>The timestamp both documented examples are signed with.</summary>
    private const long DocTimestamp = 1645423376532;

    /// <summary>
    /// Initializes a new instance of the <see cref="WsApiRequestBuilderTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public WsApiRequestBuilderTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// The signature of the documented all-ASCII example is the value the documentation prints for it.
    /// </summary>
    /// <remarks>
    /// Nine parameters added in an order that is not their sorted order, so this fails if the payload is
    /// built in insertion order - which is what a request signed like a REST one would do.
    /// </remarks>
    [Fact]
    public void AsciiExample_ProducesTheDocumentedSignature()
    {
        // arrange
        var signatureService = new DocSignatureService();

        // act
        var frame = Parse(
            WsApiRequestBuilder.BuildSigned(
                "4885f793-e5ad-4c3b-8f6c-55d891472b71",
                "order.place",
                signatureService,
                new Dictionary<string, WsApiParam>
                {
                    ["type"] = WsApiParam.Text("LIMIT"),
                    ["symbol"] = WsApiParam.Text("BTCUSDT"),
                    ["side"] = WsApiParam.Text("SELL"),
                    ["timeInForce"] = WsApiParam.Text("GTC"),
                    ["quantity"] = WsApiParam.Text("0.01000000"),
                    ["price"] = WsApiParam.Text("52000.00"),
                    ["recvWindow"] = WsApiParam.Number(100),
                }
            )
        );

        // assert
        frame
            .GetProperty("params")
            .GetProperty("signature")
            .GetString()
            .Is("aa1b5712c094bc4e57c05a1a5c1fd8d88dcd628338ea863fec7b88e59fe2db24");
    }

    /// <summary>
    /// The signature of the documented non-ASCII example is the value the documentation prints for it.
    /// </summary>
    /// <remarks>
    /// The one test that can tell a correct payload from a percent-encoded one. Every other value in this
    /// API survives encoding unchanged, so a builder that escaped its values would pass the example above
    /// and fail only against a venue, on the symbols where it matters.
    /// </remarks>
    [Fact]
    public void NonAsciiExample_IsSignedRawAndInUtf8()
    {
        // arrange
        var signatureService = new DocSignatureService();

        // act
        var frame = Parse(
            WsApiRequestBuilder.BuildSigned(
                "4885f793-e5ad-4c3b-8f6c-55d891472b71",
                "order.place",
                signatureService,
                new Dictionary<string, WsApiParam>
                {
                    ["symbol"] = WsApiParam.Text("１２３４５６"),
                    ["side"] = WsApiParam.Text("BUY"),
                    ["type"] = WsApiParam.Text("LIMIT"),
                    ["timeInForce"] = WsApiParam.Text("GTC"),
                    ["quantity"] = WsApiParam.Text("1.00000000"),
                    ["price"] = WsApiParam.Text("0.10000000"),
                    ["recvWindow"] = WsApiParam.Number(5000),
                }
            )
        );

        // assert
        frame
            .GetProperty("params")
            .GetProperty("signature")
            .GetString()
            .Is("b33892ae8e687c939f4468c6268ddd4c40ac1af18ad19a064864c47bae0752cd");
    }

    /// <summary>
    /// The account's key and the timestamp are added by the builder, so no caller has to remember them.
    /// </summary>
    [Fact]
    public void EverySignedRequest_CarriesTheKeyAndTheTimestamp()
    {
        // arrange
        var signatureService = new DocSignatureService();

        // act
        var parameters = Parse(WsApiRequestBuilder.BuildSigned("7", "session.status", signatureService))
            .GetProperty("params");

        // assert
        parameters.GetProperty("apiKey").GetString().Is(DocKey);
        parameters.GetProperty("timestamp").GetInt64().Is(DocTimestamp);
    }

    /// <summary>
    /// A number goes out as a JSON number and text as a JSON string, so the frame matches what was signed.
    /// </summary>
    /// <remarks>
    /// Both would sign identically, since the payload is text either way, and a venue reading the frame as
    /// JSON sees two different requests. Which makes this the failure that looks like a bad signature and
    /// is not.
    /// </remarks>
    [Fact]
    public void ANumberIsANumber_AndTextIsAString()
    {
        // arrange
        // act
        var parameters = Parse(
                WsApiRequestBuilder.Build(
                    "7",
                    "some.method",
                    new Dictionary<string, WsApiParam>
                    {
                        ["counted"] = WsApiParam.Number(42),
                        ["named"] = WsApiParam.Text("42"),
                    }
                )
            )
            .GetProperty("params");

        // assert
        parameters.GetProperty("counted").ValueKind.Is(JsonValueKind.Number);
        parameters.GetProperty("named").ValueKind.Is(JsonValueKind.String);
    }

    /// <summary>
    /// A method taking no parameters sends no <c>params</c> member at all.
    /// </summary>
    /// <remarks>
    /// The methods documented without parameters are documented without the member, not with an empty
    /// object, and the two are different frames.
    /// </remarks>
    [Fact]
    public void AMethodWithNoParameters_SendsNoParamsMember()
    {
        // arrange
        // act
        var frame = Parse(WsApiRequestBuilder.Build("7", "userDataStream.subscribe"));

        // assert
        frame.GetProperty("id").GetString().Is("7");
        frame.GetProperty("method").GetString().Is("userDataStream.subscribe");
        frame.TryGetProperty("params", out _).IsFalse("an empty params member was sent");
    }

    /// <summary>
    /// Parses a built frame.
    /// </summary>
    /// <param name="frame">The frame as built.</param>
    /// <returns>Its root element.</returns>
    private static JsonElement Parse(ReadOnlyMemory<byte> frame) => JsonDocument.Parse(frame).RootElement.Clone();

    /// <summary>
    /// A signature service holding the venue's own illustrative key pair, so the signatures it produces are
    /// the ones the documentation prints.
    /// </summary>
    private sealed class DocSignatureService : ISignatureService
    {
        /// <summary>Gets the timestamp both documented examples use.</summary>
        public long ServerTime => DocTimestamp;

        /// <summary>Gets the documented example API key.</summary>
        /// <returns>The key.</returns>
        public string GetKey() => DocKey;

        /// <summary>Signs with the documented example secret, exactly as the venue describes.</summary>
        /// <param name="data">The signature payload.</param>
        /// <returns>The lowercase hexadecimal signature.</returns>
        public string GetSignature(string data)
        {
            using var hash = new HMACSHA256(Encoding.UTF8.GetBytes(DocSecret));

            return Convert.ToHexString(hash.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
        }
    }
}
