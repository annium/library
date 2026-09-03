using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

namespace Annium.AI.SemanticKernel;

/// <summary>
/// Builder extensions for provider-independent plugin sources: locally implemented plugins and MCP servers.
/// </summary>
public static class SemanticKernelBuilderBaseExtensions
{
    /// <summary>
    /// Registers every discovered <see cref="ISemanticKernelPlugin"/> implementation and exposes them to the
    /// kernel as a plugin collection, each under its friendly type name.
    /// </summary>
    /// <param name="builder">The kernel builder to register into.</param>
    /// <returns>The builder, for chaining.</returns>
    public static ISemanticKernelBuilder WithPluginInstances(this ISemanticKernelBuilder builder)
    {
        builder.Container.AddAll().AssignableTo<ISemanticKernelPlugin>().As<ISemanticKernelPlugin>().Singleton();
        builder
            .Container.Add(static sp =>
            {
                var pluginInstances = sp.Resolve<IEnumerable<ISemanticKernelPlugin>>();
                var plugins = pluginInstances
                    .Select(x => KernelPluginFactory.CreateFromObject(x, x.GetType().FriendlyName()))
                    .ToArray();

                return new KernelPluginCollection(plugins);
            })
            .AsSelf()
            .Singleton();

        return builder;
    }

    /// <summary>
    /// Connects to an MCP server over streamable HTTP, exposes its tools as kernel functions under
    /// <paramref name="name"/>, and registers both the client and the resulting plugin collection.
    /// </summary>
    /// <remarks>
    /// Connecting and listing tools happen here, while registration is still running, rather than inside a
    /// DI factory: the previous version blocked a thread on <c>Task.Result</c> during service resolution
    /// (suppressed as VSTHRD002) and reported a server that was down as an <see cref="AggregateException"/>
    /// thrown by whichever component happened to resolve the kernel first. Service packs are asynchronous
    /// since 1.1.40, so this can be awaited from <c>ConfigureAsync</c>/<c>RegisterAsync</c> directly.
    /// </remarks>
    /// <param name="builder">The kernel builder to register into.</param>
    /// <param name="name">The plugin name the server's tools are grouped under.</param>
    /// <param name="url">The MCP server endpoint.</param>
    /// <param name="ct">The token that cancels connecting and listing tools.</param>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is already taken by another MCP server on this builder.
    /// </exception>
    public static async Task<ISemanticKernelBuilder> WithMcpFunctionsFromHttpServerAsync(
        this ISemanticKernelBuilder builder,
        string name,
        string url,
        CancellationToken ct = default
    )
    {
        // rejected before connecting, so the second server's socket is never opened: a repeated name would
        // otherwise register a second keyed client that shadows the first, leaving the first one resolvable
        // by nobody — and therefore never disposed — while the duplicate plugin name broke the kernel anyway
        if (
            builder.Container.Collection.Any(x =>
                x.ServiceType == typeof(McpClient) && x.IsKeyedService && Equals(x.ServiceKey, name)
            )
        )
            throw new ArgumentException($"MCP server '{name}' is already registered", nameof(name));

        var client = await McpClient.CreateAsync(
            new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Name = name,
                    Endpoint = new Uri(url),
                    TransportMode = HttpTransportMode.StreamableHttp,
                }
            ),
            cancellationToken: ct
        );

        KernelPluginCollection plugins;
        try
        {
            // the client is connected from here on, but nothing owns it until the registrations below are
            // in place: a failure in between (a cancelled token, a server that drops after the handshake)
            // would otherwise walk away from a live session nobody can reach to close
            var tools = await client.ListToolsAsync(cancellationToken: ct);
            var functions = tools.Select(x => x.AsKernelFunction()).ToArray();

            plugins = new KernelPluginCollection();
            plugins.AddFromFunctions(name, functions);
        }
        catch
        {
            await client.DisposeAsync();

            throw;
        }

        builder.Container.Add((_, _) => client).AsKeyedSelf(name).Singleton();
        // the plugin collection resolves the client rather than closing over it: the container only tracks
        // a factory-built singleton for disposal once something has actually resolved it, and nothing on
        // the ordinary path (resolve the kernel, call its functions) would otherwise ask for the client —
        // leaving it, and its open connection to the MCP server, alive for the life of the process
        builder
            .Container.Add(sp =>
            {
                sp.ResolveKeyed<McpClient>(name);

                return plugins;
            })
            .AsSelf()
            .Singleton();

        return builder;
    }
}
