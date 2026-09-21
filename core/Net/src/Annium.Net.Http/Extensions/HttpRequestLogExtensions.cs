using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

/// <summary>
/// Extension methods for adding logging to HTTP requests
/// </summary>
public static class HttpRequestLogExtensions
{
    /// <summary>
    /// How much of a response body is written to the log.
    /// </summary>
    /// <remarks>
    /// Generous enough for an account, an order list or an error, which is what body logging is for, and
    /// far below a reference payload, which is what it must not become.
    /// </remarks>
    private const int MaxLoggedBodyLength = 8 * 1024;

    /// <summary>
    /// Adds logging capabilities to the HTTP request
    /// </summary>
    /// <typeparam name="T">The type of the log subject</typeparam>
    /// <param name="request">The HTTP request</param>
    /// <param name="subject">The log subject to write logs to</param>
    /// <param name="log">The type of data to log</param>
    /// <param name="headerMasks">Header names to include in logging</param>
    /// <returns>The HTTP request with logging interceptor attached</returns>
    public static IHttpRequest WithLogFrom<T>(
        this IHttpRequest request,
        T subject,
        LogData log = default,
        string[]? headerMasks = null
    )
        where T : ILogSubject =>
        request.Intercept(async next =>
        {
            var id = Guid.NewGuid();
            var response = default(IHttpResponse);
            try
            {
                var headers = string.Empty;
                if (log.HasFlag(LogData.Headers))
                {
                    var sb = new StringBuilder($"{Environment.NewLine}Headers:");
                    IEnumerable<KeyValuePair<string, IEnumerable<string>>> headerPairs = headerMasks is null
                        ? request.Headers
                        : request.Headers.Where(x =>
                            headerMasks.Any(m => x.Key.Contains(m, StringComparison.InvariantCultureIgnoreCase))
                        );
                    foreach (var (name, values) in headerPairs)
                        sb.AppendLine($"- {name}: {string.Join(", ", values)}");
                    headers = sb.ToString();
                }

                subject.Trace<Guid, HttpMethod, Uri, string>(
                    "request {id}: {method} {uri}{headers}",
                    id,
                    request.Method,
                    request.Uri,
                    headers
                );

                response = await next();

                return response;
            }
            catch (Exception e)
            {
                subject.Trace("failed {id}: {method} {uri}: {e}", id, request.Method, request.Uri, e);
                throw;
            }
            finally
            {
                if (response is not null)
                {
                    var headers = string.Empty;
                    if (log.HasFlag(LogData.Headers))
                    {
                        var sb = new StringBuilder($"{Environment.NewLine}Headers:");
                        IEnumerable<KeyValuePair<string, IEnumerable<string>>> headerPairs = headerMasks is null
                            ? response.Headers
                            : response.Headers.Where(x =>
                                headerMasks.Any(m => x.Key.Contains(m, StringComparison.InvariantCultureIgnoreCase))
                            );
                        foreach (var (name, values) in headerPairs)
                            sb.AppendLine($"- {name}: {string.Join(", ", values)}");
                        headers = sb.ToString();
                    }
                    subject.Trace<Guid, HttpMethod, Uri, HttpStatusCode, string, string>(
                        "response {id}: {method} {uri} -> {statusCode} ({statusText}){headers}",
                        id,
                        request.Method,
                        request.Uri,
                        response.StatusCode,
                        response.StatusText,
                        headers
                    );

                    // guarded on the level, not only on the flag: reading the content materialises the
                    // whole body as a string, and an argument is evaluated before anything looks at
                    // whether the level would discard it. Unguarded, asking for body logging costs the
                    // allocation on every response whether or not a single line is ever written
                    if (log.HasFlag(LogData.Response) && LogConfig.IsEnabled(LogLevel.Trace))
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        subject.Trace<string>("response body: {body}", Truncate(body));
                    }
                }
            }
        });

    /// <summary>
    /// Caps a logged body, so one large answer cannot turn a log into the answer.
    /// </summary>
    /// <remarks>
    /// A reference payload can run to megabytes, and a log line that size is not read by anyone - it is
    /// scrolled past, and it makes the lines around it unreadable too. The cut says how much was cut, so
    /// a reader can tell a truncated body from a short one. Where the whole payload is the point, a probe
    /// that writes it to a file is the tool for that rather than the log.
    /// </remarks>
    /// <param name="body">The body as read.</param>
    /// <returns>The body, or its first part with a note saying so.</returns>
    private static string Truncate(string body) =>
        body.Length <= MaxLoggedBodyLength
            ? body
            : $"{body[..MaxLoggedBodyLength]}… [{body.Length - MaxLoggedBodyLength} more chars of {body.Length}]";
}

/// <summary>
/// Specifies which parts of an HTTP request/response to include in trace logging.
/// </summary>
[Flags]
public enum LogData
{
    /// <summary>
    /// Log neither headers nor the response body (default).
    /// </summary>
    None = 0,

    /// <summary>
    /// Include request/response headers in the log output.
    /// </summary>
    Headers = 1 << 0,

    /// <summary>
    /// Include the response body in the log output.
    /// </summary>
    Response = 1 << 1,
}
