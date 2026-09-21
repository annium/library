using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Tests.Shared.HttpExtensions;

/// <summary>
/// Verifies what <c>WithLogFromWithHeaders</c> puts in the log: the rate-limit headers, and only those.
/// </summary>
/// <remarks>
/// The header list is an allow-list, not a redaction list — the underlying extension keeps the headers
/// whose names match and drops every other. The name it carries says the opposite, which is why this is
/// asserted from both sides: a header that must appear, and a header that must not.
///
/// Worth defending because the log of a live run is the only record of what the exchange answered, and
/// losing the rate-limit headers from it is invisible until someone needs them, at which point the run
/// is over.
/// </remarks>
public class HttpRequestLogExtensionsTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequestLogExtensionsTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public HttpRequestLogExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddHttpRequestFactory(true));
    }

    /// <summary>
    /// The rate-limit headers reach the log, and the headers beside them do not.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task LoggedHeaders_AreTheRateLimitOnesAndNothingElse()
    {
        // arrange
        OverrideLogLevel(LogLevel.Trace);

        await using var server = this.RunHttpServer(
            (_, response) =>
            {
                response.Headers.Add("x-mbx-used-weight-1m", "42");
                response.Headers.Add("x-mbx-order-count-1m", "7");
                response.Headers.Add("x-some-other-header", "irrelevant");
                response.Ok();

                return Task.CompletedTask;
            }
        );

        // act
        await this.CreateHttpRequest(server)
            .Get("log")
            .WithLogFromWithHeaders(this, LogData.Headers)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        var logged = string.Join("\n", Logs.Select(x => x.Message));

        logged.Contains("x-mbx-used-weight-1m").IsTrue("the used-weight header did not reach the log");
        logged.Contains("x-mbx-order-count-1m").IsTrue("the order-count header did not reach the log");
        logged
            .Contains("x-some-other-header")
            .IsFalse("every response header reached the log, so the list selects nothing");
    }

    /// <summary>
    /// Without asking for headers, none are logged — the request and response lines still are.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WithoutTheHeadersFlag_NoHeaderReachesTheLog()
    {
        // arrange
        OverrideLogLevel(LogLevel.Trace);

        await using var server = this.RunHttpServer(
            (_, response) =>
            {
                response.Headers.Add("x-mbx-used-weight-1m", "42");
                response.Ok();

                return Task.CompletedTask;
            }
        );

        // act
        await this.CreateHttpRequest(server)
            .Get("log")
            .WithLogFromWithHeaders(this)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        var logged = string.Join("\n", Logs.Select(x => x.Message));

        logged.Contains("response ").IsTrue("the response was not logged at all");
        logged.Contains("x-mbx-used-weight-1m").IsFalse("headers were logged without being asked for");
    }

    /// <summary>
    /// A response body reaches the log when it is asked for.
    /// </summary>
    /// <remarks>
    /// Asked for on the read paths as of 2026-09-21, because the log of a live run is the only record of
    /// what the venue answered - and an attempt to read an account back from one proved nothing at all
    /// when the bodies were not in it.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WithTheResponseFlag_TheBodyReachesTheLog()
    {
        // arrange
        OverrideLogLevel(LogLevel.Trace);

        await using var server = this.RunHttpServer(
            async (_, response) =>
            {
                response.Ok();
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("""{"answer":"in the body"}"""));
            }
        );

        // act
        await this.CreateHttpRequest(server)
            .Get("log")
            .WithLogFromWithHeaders(this, LogData.Response)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        var logged = string.Join("\n", Logs.Select(x => x.Message));

        logged.Contains("in the body").IsTrue("the body did not reach the log");
    }

    /// <summary>
    /// Without asking for it, the body is not logged.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WithoutTheResponseFlag_TheBodyDoesNotReachTheLog()
    {
        // arrange
        OverrideLogLevel(LogLevel.Trace);

        await using var server = this.RunHttpServer(
            async (_, response) =>
            {
                response.Ok();
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("""{"answer":"in the body"}"""));
            }
        );

        // act
        await this.CreateHttpRequest(server)
            .Get("log")
            .WithLogFromWithHeaders(this, LogData.Headers)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        var logged = string.Join("\n", Logs.Select(x => x.Message));

        logged.Contains("response ").IsTrue("the response was not logged at all");
        logged.Contains("in the body").IsFalse("the body was logged without being asked for");
    }

    /// <summary>
    /// A body larger than the cap is cut, and the log says how much was cut.
    /// </summary>
    /// <remarks>
    /// The reason body logging is safe to turn on everywhere. A reference payload from this venue runs to
    /// megabytes, and a log line that size is not read - it is scrolled past, taking the lines around it
    /// with it. Saying how much was cut is what lets a reader tell a truncated body from a short one.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ALargeBody_IsCutAndSaysSo()
    {
        // arrange
        OverrideLogLevel(LogLevel.Trace);

        // comfortably past the cap, and made of one repeated character so the assertion below is about
        // the length rather than about where a boundary happened to land
        const int size = 40 * 1024;
        var payload = new string('x', size);

        await using var server = this.RunHttpServer(
            async (_, response) =>
            {
                response.Ok();
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(payload));
            }
        );

        // act
        await this.CreateHttpRequest(server)
            .Get("log")
            .WithLogFromWithHeaders(this, LogData.Response)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        var logged = string.Join("\n", Logs.Select(x => x.Message));

        logged.Length.IsLess(size, "the whole body reached the log, so nothing was cut");
        logged.Contains($"of {size}").IsTrue("the cut did not say how much there was");
    }
}
