using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;
using Annium.Logging;
using Annium.Net.Http;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;

/// <summary>Extension methods enforcing Binance's per-minute request weight limit on outgoing requests.</summary>
public static partial class HttpRequestRateExtensions
{
    /// <summary>
    /// Rejects the request locally with <see cref="HttpStatusCode.TooManyRequests"/> if the 1-minute request weight
    /// limit has been reached; otherwise sends it and records the weight used from Binance's <c>x-mbx-used-weight-1m</c> response header.
    /// </summary>
    /// <remarks>
    /// A local rejection answers in the shape Binance answers a rate limit with, so it travels the same route
    /// as a real one. It used to answer with a body of <c>null</c>: neither shape of the response union could
    /// read that, so the caller was told its response had failed to parse - and the rejection this method
    /// exists to make, whose whole point is to say "wait", arrived as an unexplained parse error.
    /// </remarks>
    /// <param name="request">The request to rate-limit.</param>
    /// <param name="rateLimiter">The rate limiter tracking the 1-minute request weight budget.</param>
    /// <returns>The request, for chaining.</returns>
    public static IHttpRequest WithRateDelay1M(this IHttpRequest request, IRateLimiter rateLimiter) =>
        request.Intercept(async next =>
        {
            if (!rateLimiter.CanExecute())
            {
                var error =
                    $"{{\"code\":{OperationResult.TooManyRequests},\"msg\":\"Rate limit reached locally, request not sent\"}}";
                var content = new StringContent(error);
                content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

                return HttpResponse.Result(
                    false,
                    request.Uri,
                    HttpStatusCode.TooManyRequests,
                    "Rate limit reached",
                    HttpResponse.EmptyHeaders,
                    content
                );
            }

            var response = await next();

            // a provider that has stopped answering on purpose says for how long. Weight accounting cannot
            // see that - a ban answers without the header weight is read from - so without this every caller
            // rediscovers the ban with a request of its own, and those requests are what it gets extended for
            var isRefusal = response.StatusCode is HttpStatusCode.TooManyRequests or (HttpStatusCode)418;
            if (isRefusal)
            {
                // every refusal costs a pause, including one that names no deadline - see ReadPauseAsync
                var pause = await ReadPauseAsync(response);
                request.Warn<double>("refused until further notice, pausing for {seconds}s", pause.TotalSeconds);
                rateLimiter.Block(pause);
            }

            var headerName = "x-mbx-used-weight-1m";
            var usedHeader = response.Headers.FirstOrDefault(x =>
                x.Key.Equals(headerName, StringComparison.InvariantCultureIgnoreCase)
            );
            var usedHeaderValue = usedHeader.Value?.ToArray() ?? [];
            if (usedHeaderValue.Length == 0)
            {
                // no weight to set either way; what differs is whether the absence is news. A refusal answers
                // without the header - the comment above says so, and the code counts on it - so on that path
                // an Error is noise printed at exactly the moment the log matters most. Anywhere else the
                // absence means the limiter is running blind, which is worth saying loudly
                if (isRefusal)
                    request.Trace<string>(
                        "{headerName} header not present, as a refusal does not carry it",
                        headerName
                    );
                else
                    request.Error<string>("{headerName} header not present", headerName);

                return response;
            }

            if (!int.TryParse(usedHeaderValue[0], out var used))
            {
                // if failed to parse header - also don't set weight used, but log as error
                request.Error<string, string>(
                    "{headerName} header failed to parse from {usedHeader}",
                    headerName,
                    usedHeaderValue[0]
                );
                return response;
            }

            rateLimiter.UsedWeight(used);

            return response;
        });

    /// <summary>The longest pause a stated deadline can buy, matching the longest ban Binance documents.</summary>
    /// <remarks>
    /// Binance documents IP bans as scaling "from 2 minutes to 3 days" for repeat offenders. This used to be
    /// one hour, which is below the documented maximum by a factor of 72: on a long ban the client resumed
    /// after an hour and resumed straight into it - which the same page names as the cause of longer bans. The
    /// cap exists so a garbled or hostile value costs a pause rather than the rest of the process's life, so
    /// it stays; it is the value that was wrong, not the idea.
    /// </remarks>
    private static readonly TimeSpan _maxPause = TimeSpan.FromDays(3);

    /// <summary>How long to stand down after a refusal that states no deadline at all.</summary>
    /// <remarks>
    /// A refusal without a deadline used to produce no pause whatsoever: the deadline readers returned zero
    /// and the caller only blocked on a positive value, so the limiter carried on as though nothing had
    /// happened. That is the worst moment to keep going, and it is not rare - a system-level throttle says
    /// only "please try again", and a plain 429 carries no deadline either.
    /// <para>
    /// One minute, because the weight budget these refusals guard is a per-minute window: waiting for it to
    /// roll is the shortest pause that can actually clear the condition. A 418 ban outlasts it, but the ban
    /// message states its own deadline and so never reaches this fallback.
    /// </para>
    /// </remarks>
    private static readonly TimeSpan _defaultPause = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Reads how long the provider wants to be left alone for, from the standard <c>Retry-After</c> header or,
    /// failing that, from the deadline Binance states in a ban message - and falls back to a fixed pause when
    /// the refusal states nothing at all.
    /// </summary>
    /// <remarks>
    /// The deadline is the provider's own clock, compared against ours - good enough when the wait is tens of
    /// minutes and the two are seconds apart.
    /// </remarks>
    /// <param name="response">The refusing response.</param>
    /// <returns>How long to pause for, never zero: a refusal always costs a pause.</returns>
    private static async Task<TimeSpan> ReadPauseAsync(IHttpResponse response)
    {
        var retryAfter = response
            .Headers.FirstOrDefault(x => x.Key.Equals("Retry-After", StringComparison.InvariantCultureIgnoreCase))
            .Value?.FirstOrDefault();
        if (int.TryParse(retryAfter, out var seconds) && seconds > 0)
            return TimeSpan.FromSeconds(Math.Min(seconds, _maxPause.TotalSeconds));

        var content = await response.Content.ReadAsStringAsync();
        var match = BannedUntil().Match(content);
        if (!match.Success || !long.TryParse(match.Groups[1].Value, out var until))
            return _defaultPause;

        var pause = DateTimeOffset.FromUnixTimeMilliseconds(until) - DateTimeOffset.UtcNow;

        // a deadline already in the past says the ban has lapsed, not that we may resume instantly - the
        // clocks differ, and the cheapest way to be wrong here is to be first through the door
        if (pause < _defaultPause)
            return _defaultPause;

        return pause > _maxPause ? _maxPause : pause;
    }

    /// <summary>
    /// Matches the deadline in Binance's ban message, e.g. <c>banned until 1788809789789</c>.
    /// </summary>
    /// <returns>The compiled expression.</returns>
    [GeneratedRegex(@"banned until (\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex BannedUntil();
}
