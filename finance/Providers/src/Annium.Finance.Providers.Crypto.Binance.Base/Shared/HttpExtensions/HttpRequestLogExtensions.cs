using Annium.Logging;
using Annium.Net.Http;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;

/// <summary>Extension methods for logging Binance HTTP requests, keeping only the venue's rate-limit headers.</summary>
public static class HttpRequestLogExtensions
{
    /// <summary>
    /// The header name prefixes worth logging: the used-weight and order-count ones.
    /// </summary>
    /// <remarks>
    /// This is an allow-list and not a redaction list, whatever the underlying parameter is called -
    /// the extension keeps the headers whose names match one of these and drops every other. The
    /// documentation here said the opposite for as long as nothing exercised it, which is how a
    /// comment gets to contradict the line below it.
    /// </remarks>
    private static readonly string[] _headerMasks = ["x-mbx-used-weight", "x-mbx-order"];

    /// <summary>
    /// Attaches request/response logging to the request, logging the <c>x-mbx-used-weight</c> and
    /// <c>x-mbx-order</c> headers and no others.
    /// </summary>
    /// <typeparam name="T">The type of the logging subject.</typeparam>
    /// <param name="request">The request to attach logging to.</param>
    /// <param name="subject">The subject the log entries are attributed to.</param>
    /// <param name="log">The parts of the request/response to log.</param>
    /// <returns>The request, for chaining.</returns>
    public static IHttpRequest WithLogFromWithHeaders<T>(this IHttpRequest request, T subject, LogData log = default)
        where T : ILogSubject
    {
        return request.WithLogFrom(subject, log, _headerMasks);
    }
}
