using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Net.Http;
using Annium.Net.Servers.Web;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Tests.Shared.HttpExtensions;

/// <summary>
/// Verifies that <c>WithRateDelay1M</c> consults the rate limiter before a request goes out - short-circuiting
/// to 429 without touching the server when the limiter refuses - and afterwards feeds the
/// <c>x-mbx-used-weight-1m</c> response header back into it, ignoring the header when it's absent or malformed.
/// </summary>
public class HttpRequestRateExtensionsTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequestRateExtensionsTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public HttpRequestRateExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddHttpRequestFactory(true));
    }

    /// <summary>
    /// When the limiter reports it can't execute, the request short-circuits to 429 and never reaches the
    /// server, and no used-weight sample is recorded.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CantExecute_ReturnsTooManyRequestsAndSkipsServer()
    {
        // arrange
        var limiter = new FakeRateLimiter(false);
        var serverHits = 0;

        await using var server = this.RunHttpServer(
            (_, response) =>
            {
                serverHits++;
                response.Ok();
                return Task.CompletedTask;
            }
        );

        // act
        var response = await SendAsync(server, limiter);

        // assert
        response.StatusCode.Is(HttpStatusCode.TooManyRequests);
        serverHits.Is(0);
        limiter.UsedWeights.IsEmpty();
    }

    /// <summary>
    /// A numeric used-weight header is parsed and recorded on the limiter, matched case-insensitively since
    /// Binance's own casing for the header varies.
    /// </summary>
    /// <param name="header">The header name to send the weight under.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("x-mbx-used-weight-1m")]
    [InlineData("X-MBX-Used-Weight-1M")]
    public async Task ValidHeader_SetsUsedWeight(string header)
    {
        // arrange
        var limiter = new FakeRateLimiter(true);

        await using var server = this.RunHttpServer(
            (_, response) =>
            {
                response.Headers.Add(header, "123");
                response.Ok();
                return Task.CompletedTask;
            }
        );

        // act
        var httpResponse = await SendAsync(server, limiter);

        // assert
        httpResponse.StatusCode.Is(HttpStatusCode.OK);
        limiter.UsedWeights.Has(1);
        limiter.UsedWeights.At(0).Is(123);
    }

    /// <summary>
    /// When the response carries no used-weight header, the limiter is left untouched.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task MissingHeader_DoesNotUpdateWeight()
    {
        // arrange
        var limiter = new FakeRateLimiter(true);

        await using var server = this.RunHttpServer(
            (_, response) =>
            {
                response.Ok();
                return Task.CompletedTask;
            }
        );

        // act
        await SendAsync(server, limiter);

        // assert
        limiter.UsedWeights.IsEmpty();
    }

    /// <summary>
    /// When the used-weight header value isn't a number, it's ignored and the limiter is left untouched.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task InvalidHeaderValue_DoesNotUpdateWeight()
    {
        // arrange
        var limiter = new FakeRateLimiter(true);

        await using var server = this.RunHttpServer(
            (_, response) =>
            {
                response.Headers.Add("x-mbx-used-weight-1m", "abc");
                response.Ok();
                return Task.CompletedTask;
            }
        );

        // act
        await SendAsync(server, limiter);

        // assert
        limiter.UsedWeights.IsEmpty();
    }

    /// <summary>
    /// A refusal that states a deadline pauses the limiter until then. Every caller would otherwise
    /// rediscover the ban with a request of its own, and those requests are what a ban is extended for.
    /// </summary>
    /// <param name="code">The status the provider refuses with.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(429)]
    [InlineData(418)]
    public async Task StatedBanDeadline_PausesTheLimiter(int code)
    {
        // arrange - banned for the next ten minutes, in Binance's own wording
        var limiter = new FakeRateLimiter(true);
        var until = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeMilliseconds();

        var body = $$"""{"code":-1003,"msg":"Way too many requests; IP(1.2.3.4) banned until {{until}}."}""";
        await using var server = this.RunHttpServerWithResponse(
            (HttpStatusCode)code,
            MediaTypeNames.Application.Json,
            body
        );

        // act
        await SendAsync(server, limiter);

        // assert
        limiter.Blocks.Has(1);
        (limiter.Blocks[0] > TimeSpan.FromMinutes(9)).IsTrue($"paused for only {limiter.Blocks[0]}");
        (limiter.Blocks[0] <= TimeSpan.FromMinutes(10)).IsTrue($"paused for {limiter.Blocks[0]}");
    }

    /// <summary>
    /// A refusal carrying the standard Retry-After header is honoured by it, without reading the body.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RetryAfterHeader_PausesTheLimiter()
    {
        // arrange
        var limiter = new FakeRateLimiter(true);

        await using var server = this.RunHttpServer(
            (_, response) =>
            {
                response.StatusCode((HttpStatusCode)429);
                response.Headers.Add("Retry-After", "30");

                return Task.CompletedTask;
            }
        );

        // act
        await SendAsync(server, limiter);

        // assert
        limiter.Blocks.Has(1);
        limiter.Blocks[0].Is(TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// A refusal that says nothing about how long it lasts pauses nothing: guessing a duration is worse than
    /// leaving the weight accounting to it.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RefusalWithoutADeadline_PausesNothing()
    {
        // arrange
        var limiter = new FakeRateLimiter(true);

        await using var server = this.RunHttpServerWithResponse(
            HttpStatusCode.TooManyRequests,
            MediaTypeNames.Text.Html,
            "<html>too many requests</html>"
        );

        // act
        await SendAsync(server, limiter);

        // assert
        limiter.Blocks.IsEmpty();
    }

    /// <summary>
    /// A response that is not a refusal is never read for a deadline, however it is worded.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SuccessfulResponse_PausesNothing()
    {
        // arrange
        var limiter = new FakeRateLimiter(true);

        await using var server = this.RunHttpServer(
            (_, response) =>
            {
                response.Ok();

                return Task.CompletedTask;
            }
        );

        // act
        await SendAsync(server, limiter);

        // assert
        limiter.Blocks.IsEmpty();
    }

    /// <summary>
    /// Sends a GET request through the rate-limit extension against the given server and limiter.
    /// </summary>
    /// <param name="server">The server to send the request to.</param>
    /// <param name="limiter">The rate limiter to gate and record weight through.</param>
    /// <returns>The HTTP response.</returns>
    private Task<IHttpResponse> SendAsync(IServer server, FakeRateLimiter limiter)
    {
        var request = this.CreateHttpRequest(server).Get("rate-limit").WithRateDelay1M(limiter);

        return request.RunAsync();
    }

    /// <summary>
    /// A scriptable <see cref="IRateLimiter"/> that returns a fixed <see cref="CanExecute"/> answer and
    /// records every weight it's given, so tests can assert on both sides of the rate-limit extension.
    /// </summary>
    private sealed class FakeRateLimiter : IRateLimiter
    {
        /// <summary>Gets the weights recorded via <see cref="UsedWeight"/>, in call order.</summary>
        public IReadOnlyList<int> UsedWeights => _usedWeights;

        /// <summary>Gets the pauses recorded via <see cref="Block"/>, in call order.</summary>
        public IReadOnlyList<TimeSpan> Blocks => _blocks;

        /// <summary>Backing store for <see cref="UsedWeights"/>.</summary>
        private readonly List<int> _usedWeights = [];

        /// <summary>Backing store for <see cref="Blocks"/>.</summary>
        private readonly List<TimeSpan> _blocks = [];

        /// <summary>The fixed answer returned by <see cref="CanExecute"/>.</summary>
        private readonly bool _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeRateLimiter"/> class.
        /// </summary>
        /// <param name="canExecute">The fixed answer <see cref="CanExecute"/> should return.</param>
        public FakeRateLimiter(bool canExecute)
        {
            _canExecute = canExecute;
        }

        /// <summary>Records a pause without enforcing it.</summary>
        /// <param name="duration">The pause to record.</param>
        public void Block(TimeSpan duration) => _blocks.Add(duration);

        /// <summary>Does nothing; no resources to release.</summary>
        public void Dispose() { }

        /// <summary>Returns the fixed answer configured at construction.</summary>
        /// <returns><see langword="true"/> if the caller should be allowed to execute; otherwise, <see langword="false"/>.</returns>
        public bool CanExecute() => _canExecute;

        /// <summary>Does nothing; this fake doesn't track a configurable limit.</summary>
        /// <param name="limit">The limit to apply. Ignored.</param>
        public void UpdateLimit(int limit) { }

        /// <summary>Records the given weight in <see cref="UsedWeights"/>.</summary>
        /// <param name="weight">The weight reported as used.</param>
        public void UsedWeight(int weight) => _usedWeights.Add(weight);
    }
}
