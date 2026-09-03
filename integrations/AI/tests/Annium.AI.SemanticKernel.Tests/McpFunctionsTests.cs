using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging.Shared;
using Annium.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using ModelContextProtocol.Server;
using Xunit;

namespace Annium.AI.SemanticKernel.Tests;

/// <summary>
/// Tests for MCP-backed kernel functions, driven against a real MCP server hosted in-process: the whole
/// path (streamable HTTP transport, tool listing, kernel function conversion) is exercised, and only the
/// remote's location is local.
/// </summary>
public class McpFunctionsTests
{
    /// <summary>
    /// Tools the MCP server publishes become kernel functions, grouped under the plugin name the caller
    /// chose, and the client that fetched them is resolvable under the same name.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WithMcpFunctions_ServerWithTool_ExposesItAsKernelFunction()
    {
        // arrange
        await using var server = await McpServerHost.StartAsync();
        var container = Container();

        // act
        var builder = container.AddSemanticKernel();
        await builder.WithMcpFunctionsFromHttpServerAsync("probe", server.Url, TestContext.Current.CancellationToken);
        // the provider owns the MCP client and its live connection, so it goes down before the server does
        await using var provider = container.BuildServiceProvider();
        var kernel = provider.Resolve<Kernel>();

        // assert - the plugin carries the caller's name, not the server's, and holds the server's tool
        kernel.Plugins.Has(1);
        var plugin = kernel.Plugins.Single();
        plugin.Name.Is("probe");
        // the function keeps the name the MCP server advertises, which is the tool name the protocol
        // carries (lower-cased by the server SDK), not the C# method name
        plugin.Select(x => x.Name).Has(1).At(0).Is("ping");

        // and the client is registered under the same name, so it can be disposed with the container
        provider.ResolveKeyed<McpClient>("probe").IsNotDefault();

        // calling it goes all the way to the server and back: matching names alone would still pass if the
        // function were disconnected from the client that fetched it
        var result = await kernel.InvokeAsync(
            plugin["ping"],
            new KernelArguments(),
            TestContext.Current.CancellationToken
        );
        result.ToString().Contains("pong").IsTrue("the call must reach the MCP server");
    }

    /// <summary>
    /// Two servers cannot share a plugin name: the second registration is refused up front, before it opens
    /// a connection that nothing would ever be able to resolve — or close.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WithMcpFunctions_NameAlreadyRegistered_Throws()
    {
        // arrange
        await using var server = await McpServerHost.StartAsync();
        var container = Container();
        var builder = container.AddSemanticKernel();
        await builder.WithMcpFunctionsFromHttpServerAsync("probe", server.Url, TestContext.Current.CancellationToken);

        // act & assert
        await Wrap.It(async () =>
                await builder.WithMcpFunctionsFromHttpServerAsync(
                    "probe",
                    server.Url,
                    TestContext.Current.CancellationToken
                )
            )
            .ThrowsAsync<ArgumentException>();

        // the first call's client is live; resolving it through the container is what puts it under the
        // container's disposal, so the connection closes with the provider instead of being dropped
        await using var provider = container.BuildServiceProvider();
        provider.Resolve<Kernel>();
    }

    /// <summary>
    /// A server that cannot be reached fails the registration call itself, rather than being deferred into
    /// whichever component happens to resolve the kernel first.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WithMcpFunctions_UnreachableServer_ThrowsFromRegistration()
    {
        // arrange - a port nothing listens on; the address is reserved by binding and releasing it
        var url = McpServerHost.ReserveUnusedUrl();
        var container = Container();
        var builder = container.AddSemanticKernel();

        // act
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30));
        // act & assert - the point of awaiting the connection here is that the failure surfaces at
        // registration; ThrowsAsync fails the test itself if the call returns instead of throwing
        await Wrap.It(async () => await builder.WithMcpFunctionsFromHttpServerAsync("probe", url, cts.Token))
            .ThrowsAsync<Exception>();
    }

    /// <summary>
    /// Builds a container with the logging the kernel registrations depend on.
    /// </summary>
    /// <returns>A container ready for Semantic Kernel registrations.</returns>
    private static IServiceContainer Container()
    {
        var container = new ServiceContainer();
        container.AddLogging();
        container.Collection.AddLogging();

        return container;
    }
}

/// <summary>
/// An MCP server hosted in-process for the duration of a test, listening on a port the OS assigns.
/// </summary>
file sealed class McpServerHost : IAsyncDisposable
{
    /// <summary>
    /// The endpoint the MCP client connects to.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// The running web application serving the MCP endpoint.
    /// </summary>
    private readonly WebApplication _app;

    /// <summary>
    /// Creates the host around an already-started application.
    /// </summary>
    /// <param name="app">The running web application serving the MCP endpoint.</param>
    /// <param name="url">The endpoint the MCP client connects to.</param>
    private McpServerHost(WebApplication app, string url)
    {
        _app = app;
        Url = url;
    }

    /// <summary>
    /// Starts a server publishing the probe tools.
    /// </summary>
    /// <returns>The running host.</returns>
    public static async Task<McpServerHost> StartAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();
        // port 0 lets the listener claim a free port itself — picking one here and binding it a moment
        // later races against the servers of tests running in parallel
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddMcpServer().WithHttpTransport().WithTools<ProbeTools>();

        var app = builder.Build();
        app.MapMcp();
        await app.StartAsync();

        var url = app.Urls.First();

        return new McpServerHost(app, url);
    }

    /// <summary>
    /// Returns an address that was free a moment ago and has nothing listening on it now.
    /// </summary>
    /// <returns>An endpoint no server is bound to.</returns>
    public static string ReserveUnusedUrl()
    {
        using var probe = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        probe.Start();
        var port = ((System.Net.IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();

        return $"http://127.0.0.1:{port}";
    }

    /// <summary>
    /// Stops the server.
    /// </summary>
    /// <returns>A task that completes once the server has stopped.</returns>
    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}

/// <summary>
/// The tool surface the test server publishes.
/// </summary>
[McpServerToolType]
file sealed class ProbeTools
{
    /// <summary>
    /// Answers with a fixed string, so the test can tell the tool apart from any other.
    /// </summary>
    /// <returns>A fixed answer.</returns>
    [McpServerTool]
    [Description("Answers with a fixed string")]
    public static string Ping() => "pong";
}
