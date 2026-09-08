using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers.Web;

/// <summary>
/// Helpers for ending a request the way <see cref="HttpListener"/> expects.
/// </summary>
public static class HttpListenerContextExtensions
{
    /// <summary>
    /// Answers with the given status, consuming whatever is left of the request body first.
    /// </summary>
    /// <remarks>
    /// The draining is the point. A request refused on its headers alone - a bad token, an unknown route - is
    /// answered without its body ever being read, and <see cref="HttpListener"/> asks that a request body be
    /// consumed before its response is closed. Left undrained, what the connection does next is the
    /// listener implementation's business rather than something the handler has decided, and a client that
    /// is still uploading may be waiting on an answer that was settled in microseconds.
    /// </remarks>
    /// <param name="ctx">The context to answer on.</param>
    /// <param name="statusCode">The status to answer with.</param>
    /// <param name="ct">The cancellation token for the request.</param>
    /// <returns>A task that completes once the response has been closed.</returns>
    public static async Task CloseAsync(this HttpListenerContext ctx, HttpStatusCode statusCode, CancellationToken ct)
    {
        ctx.Response.StatusCode = (int)statusCode;

        await ctx.CloseAsync(ct);
    }

    /// <summary>
    /// Closes the response as it already stands, consuming whatever is left of the request body first.
    /// </summary>
    /// <remarks>
    /// For a handler that has already set its own status and written its own body; see the overload taking
    /// a status for one that decided on the headers alone.
    /// </remarks>
    /// <param name="ctx">The context to close.</param>
    /// <param name="ct">The cancellation token for the request.</param>
    /// <returns>A task that completes once the response has been closed.</returns>
    public static async Task CloseAsync(this HttpListenerContext ctx, CancellationToken ct)
    {
        await DrainAsync(ctx.Request, ct);

        ctx.Response.Close();
    }

    /// <summary>
    /// Reads whatever is left of the request body and discards it.
    /// </summary>
    /// <param name="request">The request whose body is drained.</param>
    /// <param name="ct">The cancellation token for the request.</param>
    /// <returns>A task that completes once the body has been consumed, or given up on.</returns>
    private static async Task DrainAsync(HttpListenerRequest request, CancellationToken ct)
    {
        if (!request.HasEntityBody)
            return;

        try
        {
            await request.InputStream.CopyToAsync(Stream.Null, ct);
        }
        catch (Exception)
        {
            // a body that cannot be drained is not worth failing an already-decided response over: the
            // client is gone, or the connection is, and either way the answer still has to be attempted
        }
    }
}
