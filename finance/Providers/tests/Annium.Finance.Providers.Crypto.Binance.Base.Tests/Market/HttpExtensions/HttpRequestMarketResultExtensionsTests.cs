using System;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Domain.Market.Operations;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Converters;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Market.HttpExtensions;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Net.Http;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Tests.Market.HttpExtensions;

/// <summary>
/// Verifies that <c>AsMarketResultAsync</c> maps HTTP outcomes - transport failures, aborted requests,
/// unparsable bodies, Binance error payloads and successful responses - onto the matching
/// <see cref="MarketOperationStatus"/>.
/// </summary>
public class HttpRequestMarketResultExtensionsTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequestMarketResultExtensionsTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public HttpRequestMarketResultExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers an HTTP request factory using the converters the responses under test are deserialized with.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.AddHttpRequestFactoryWithJsonSerializer(
            string.Empty,
            new JsonSerializerOptions()
                .ResetConverters()
                .AddConverter<ServerTimeConverter>()
                .AddConverter<OperationResultConverter>()
        );
    }

    /// <summary>
    /// A request to a port nothing is listening on - refused outright - maps to <see cref="MarketOperationStatus.NetworkError"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task NetworkError()
    {
        // arrange
        // act
        var result = await this.CreateHttpRequestToClosedPort().Get("/").AsMarketResultAsync<ServerTime>();

        // assert
        result.Status.Is(MarketOperationStatus.NetworkError);
        result.Data.IsDefault();
        result.Message.IsNotEmpty();
    }

    /// <summary>
    /// A request that times out before the server responds maps to <see cref="MarketOperationStatus.Aborted"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Abort()
    {
        // arrange
        await using var server = this.RunHttpServer((_, _) => Task.Delay(100));

        // act
        var result = await this.CreateHttpRequest(server)
            .Get("/")
            .Timeout(TimeSpan.FromMilliseconds(10))
            .AsMarketResultAsync<ServerTime>();

        // assert
        result.Status.Is(MarketOperationStatus.Aborted);
        result.Data.IsDefault();
        result.Message.IsNotEmpty();
    }

    /// <summary>
    /// A body that parses as neither shape of the response is reported by the status the server sent, when
    /// that status means something specific. "Could not read the body" is the weaker of the two accounts:
    /// a rate limit answered with a page we have no serializer for is a rate limit, and a caller that backs
    /// off on one and gives up on the other needs to be told which it was.
    /// </summary>
    /// <param name="code">The HTTP status code returned with the unparsable body.</param>
    /// <param name="status">The status the response is expected to map to.</param>
    /// <param name="body">The unparsable response body.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(HttpStatusCode.BadRequest, MarketOperationStatus.BadRequest, "not json")]
    [InlineData(HttpStatusCode.BadRequest, MarketOperationStatus.BadRequest, "{}")]
    [InlineData((HttpStatusCode)418, MarketOperationStatus.TooManyRequests, "<html>banned</html>")]
    [InlineData(HttpStatusCode.TooManyRequests, MarketOperationStatus.TooManyRequests, "not json")]
    public async Task UnparsedErrorResponse_IsReportedByItsStatus(
        HttpStatusCode code,
        MarketOperationStatus status,
        string body
    )
    {
        // arrange
        await using var server = this.RunHttpServerWithJsonResponse(code, body);

        // act
        var result = await this.CreateHttpRequest(server).Get("/").AsMarketResultAsync<ServerTime>();

        // assert
        result.Status.Is(status);
        result.Message.IsNotEmpty();
    }

    /// <summary>
    /// A body in a media type nothing is registered to read - an edge server's HTML error page, in
    /// practice - is still reported by its status, and says what it could not read rather than naming a
    /// missing service. "No keyed service for ISerializer&lt;string&gt;" described our container, not the
    /// exchange's answer, and it is what an IP ban looked like from the call site.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UnreadableMediaType_IsReportedByItsStatus()
    {
        // arrange
        await using var server = this.RunHttpServerWithResponse(
            (HttpStatusCode)418,
            MediaTypeNames.Text.Html,
            "<html><body>banned</body></html>"
        );

        // act
        var result = await this.CreateHttpRequest(server).Get("/").AsMarketResultAsync<ServerTime>();

        // assert
        result.Status.Is(MarketOperationStatus.TooManyRequests);
        result.Message.Contains("serializer").IsTrue($"message does not say what it could not read: {result.Message}");
    }

    /// <summary>
    /// A status that says nothing specific leaves the parse failure as the account of what happened - there
    /// is nothing better to replace it with.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UnparsedErrorResponse_WithAGenericStatus_StaysAParseError()
    {
        // arrange
        await using var server = this.RunHttpServerWithJsonResponse(HttpStatusCode.InternalServerError, "not json");

        // act
        var result = await this.CreateHttpRequest(server).Get("/").AsMarketResultAsync<ServerTime>();

        // assert
        result.Status.Is(MarketOperationStatus.ParseError);
        result.Message.IsNotEmpty();
    }

    /// <summary>
    /// A Binance error payload is mapped by its numeric code: a negative code to
    /// <see cref="MarketOperationStatus.BadRequest"/>, any other recognized code to
    /// <see cref="MarketOperationStatus.UnknownError"/>, with the payload message passed through unchanged.
    /// </summary>
    /// <param name="code">The Binance error code carried in the payload.</param>
    /// <param name="status">The status the code is expected to map to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(-1, MarketOperationStatus.BadRequest)]
    [InlineData(-1003, MarketOperationStatus.TooManyRequests)]
    [InlineData(10, MarketOperationStatus.UnknownError)]
    public async Task OperationResultResponse(long code, MarketOperationStatus status)
    {
        // arrange
        await using var server = this.RunHttpServerWithJsonResponse(
            HttpStatusCode.BadRequest,
            new { code, msg = "error" }
        );

        // act
        var result = await this.CreateHttpRequest(server).Get("/").AsMarketResultAsync<ServerTime>();

        // assert
        result.Status.Is(status);
        result.Message.Is("error");
    }

    /// <summary>
    /// A 200 response with a well-formed body is deserialized into its data, with an empty message and
    /// <see cref="MarketOperationStatus.Ok"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SuccessResponse()
    {
        // arrange
        await using var server = this.RunHttpServerWithJsonResponse(HttpStatusCode.OK, new { serverTime = 20 });

        // act
        var result = await this.CreateHttpRequest(server).Get("/").AsMarketResultAsync<ServerTime>();

        // assert
        result.Status.Is(MarketOperationStatus.Ok);
        result.Data.IsNotDefault();
        result.Data.Value.Is(20);
        result.Message.IsEmpty();
    }

    /// <summary>
    /// A non-2xx HTTP status is mapped to the matching status even when the body still parses as valid
    /// data, since Binance can return an error status code alongside a well-formed payload; 418 is
    /// Binance's IP-ban status and is treated the same as the standard 429.
    /// </summary>
    /// <param name="code">The HTTP status code returned by the server.</param>
    /// <param name="status">The status the code is expected to map to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(HttpStatusCode.BadRequest, MarketOperationStatus.BadRequest)]
    [InlineData((HttpStatusCode)418, MarketOperationStatus.TooManyRequests)]
    [InlineData(HttpStatusCode.TooManyRequests, MarketOperationStatus.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError, MarketOperationStatus.UnknownError)]
    public async Task FailedSuccessResponse(HttpStatusCode code, MarketOperationStatus status)
    {
        // arrange
        await using var server = this.RunHttpServerWithJsonResponse(code, new { serverTime = 20 });

        // act
        var result = await this.CreateHttpRequest(server).Get("/").AsMarketResultAsync<ServerTime>();

        // assert
        result.Status.Is(status);
        result.Data.IsNotDefault();
        result.Data.Value.Is(20);
        result.Message.IsEmpty();
    }
}
