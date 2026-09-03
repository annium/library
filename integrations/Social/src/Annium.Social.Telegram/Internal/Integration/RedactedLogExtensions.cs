using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Annium.Logging;
using Annium.Net.Http;

namespace Annium.Social.Telegram.Internal.Integration;

/// <summary>
/// Request tracing that keeps the bot's secrets out of the log.
/// </summary>
/// <remarks>
/// Every Telegram request carries the bot token in its path (<c>/bot{token}/method</c>), and some carry the
/// webhook secret in the query, so <c>WithLogFrom</c> from Annium.Net.Http — which traces the URI verbatim —
/// would write both to the log on every call. This traces the same request and response with those values
/// masked instead.
/// </remarks>
internal static partial class RedactedLogExtensions
{
    /// <summary>
    /// The placeholder every masked value is replaced with.
    /// </summary>
    private const string Mask = "***";

    /// <summary>
    /// Traces the request and its response, with the bot token and any secret query parameters masked.
    /// </summary>
    /// <typeparam name="T">The type of the log subject.</typeparam>
    /// <param name="request">The request to trace.</param>
    /// <param name="subject">The log subject the trace is written to.</param>
    /// <returns>The request, with the tracing interceptor attached.</returns>
    public static IHttpRequest WithRedactedLogFrom<T>(this IHttpRequest request, T subject)
        where T : ILogSubject =>
        request.Intercept(async next =>
        {
            var id = Guid.NewGuid();
            var uri = Redact(request.Uri);

            subject.Trace<Guid, HttpMethod, string>("request {id}: {method} {uri}", id, request.Method, uri);
            try
            {
                var response = await next();

                subject.Trace<Guid, HttpMethod, string, HttpStatusCode>(
                    "response {id}: {method} {uri} -> {statusCode}",
                    id,
                    request.Method,
                    uri,
                    response.StatusCode
                );

                return response;
            }
            catch (Exception e)
            {
                subject.Trace<Guid, HttpMethod, string, Exception>(
                    "failed {id}: {method} {uri}: {e}",
                    id,
                    request.Method,
                    uri,
                    e
                );

                throw;
            }
        });

    /// <summary>
    /// Masks the bot token in the path and the value of any secret-bearing query parameter.
    /// </summary>
    /// <param name="uri">The request URI.</param>
    /// <returns>The URI as a string, safe to log.</returns>
    private static string Redact(Uri uri)
    {
        var text = uri.ToString();
        text = TokenPattern().Replace(text, $"/bot{Mask}");
        text = SecretParamPattern().Replace(text, $"$1={Mask}");

        return text;
    }

    /// <summary>
    /// Matches the <c>/bot{token}</c> path segment every Telegram API URL starts with.
    /// </summary>
    /// <returns>The compiled pattern.</returns>
    [GeneratedRegex(@"/bot[^/?#]+")]
    private static partial Regex TokenPattern();

    /// <summary>
    /// Matches query parameters whose value is a secret.
    /// </summary>
    /// <returns>The compiled pattern.</returns>
    [GeneratedRegex(@"(?<=[?&])(secret_token|token)=[^&#]*", RegexOptions.IgnoreCase)]
    private static partial Regex SecretParamPattern();
}
