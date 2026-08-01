using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Integrations.Social.Telegram.Internal.Integration;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Net.Servers.Web;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Integrations.Social.Telegram.Tests;

/// <summary>
/// Base for tests that drive the Telegram integration against a local stand-in for the Bot API:
/// the whole path (HTTP request, serializer, domain model) is exercised, only the remote is faked.
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(true);
            container.AddHttpRequestFactory(true);
        });
    }

    /// <summary>
    /// Starts a local server answering Bot API calls and returns an <see cref="ApiContext"/> bound to it.
    /// </summary>
    /// <param name="handle">Handler receiving the called method name (e.g. getUpdates) and the query string.</param>
    /// <returns>The running server and an API context pointing at it.</returns>
    private protected (IServer Server, ApiContext Context) RunApi(Func<string, NameValueCollection, ApiReply> handle)
    {
        var sp = Get<IServiceProvider>();

        // port 0 lets the listener claim a free port itself — picking one here and binding it a moment
        // later races against the servers of tests running in parallel
        var server = ServerBuilder.New(sp).WithHttpHandler(new ApiHandler(handle, Logger)).Start().NotNull();

        var context = new ApiContext(
            new Uri($"http://{server.Host}:{server.Port}/bottest-token"),
            Get<IHttpRequestFactory>(),
            Get<ISerializer<Stream>>()
        );

        return (server, context);
    }
}

/// <summary>
/// Canned Bot API answer: raw JSON body plus the status code to serve it with.
/// </summary>
public sealed record ApiReply(string Body, HttpStatusCode StatusCode = HttpStatusCode.OK);

/// <summary>
/// Dispatches incoming requests to the test's handler by Bot API method name.
/// </summary>
file class ApiHandler : IHttpHandler, ILogSubject
{
    /// <summary>
    /// The logger used to record handling failures.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The test-supplied handler producing the reply for each call.
    /// </summary>
    private readonly Func<string, NameValueCollection, ApiReply> _handle;

    public ApiHandler(Func<string, NameValueCollection, ApiReply> handle, ILogger logger)
    {
        _handle = handle;
        Logger = logger;
    }

    /// <summary>
    /// Serves the reply the test's handler produces for the requested Bot API method.
    /// </summary>
    /// <param name="ctx">The listener context of the incoming request.</param>
    /// <param name="ct">The token that stops writing the response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        try
        {
            var method = ctx.Request.Url.NotNull().Segments[^1];
            var reply = _handle(method, ctx.Request.QueryString);

            ctx.Response.StatusCode = (int)reply.StatusCode;
            ctx.Response.ContentType = "application/json";
            var data = Encoding.UTF8.GetBytes(reply.Body);
            await ctx.Response.OutputStream.WriteAsync(data, ct);
        }
        catch (Exception e)
        {
            this.Error(e);
            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            ctx.Response.Close();
        }
    }
}
