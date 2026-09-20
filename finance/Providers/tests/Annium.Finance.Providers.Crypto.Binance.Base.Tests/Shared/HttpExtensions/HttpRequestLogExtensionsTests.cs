using System.Linq;
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
}
