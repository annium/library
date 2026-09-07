using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;
using Annium.Logging;
using Annium.Net.Http;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;

/// <summary>Extension methods enforcing Binance's per-minute request weight limit on outgoing requests.</summary>
public static class HttpRequestRateExtensions
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

            var headerName = "x-mbx-used-weight-1m";
            var usedHeader = response.Headers.FirstOrDefault(x =>
                x.Key.Equals(headerName, StringComparison.InvariantCultureIgnoreCase)
            );
            var usedHeaderValue = usedHeader.Value?.ToArray() ?? [];
            if (usedHeaderValue.Length == 0)
            {
                // if failed to fetch header - don't set any weight used, but log as error
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
}
