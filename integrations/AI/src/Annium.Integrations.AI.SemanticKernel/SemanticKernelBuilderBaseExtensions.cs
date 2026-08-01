using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

namespace Annium.Integrations.AI.SemanticKernel;

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
    public static async Task<ISemanticKernelBuilder> WithMcpFunctionsFromHttpServerAsync(
        this ISemanticKernelBuilder builder,
        string name,
        string url,
        CancellationToken ct = default
    )
    {
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

        var tools = await client.ListToolsAsync(cancellationToken: ct);
        var functions = tools.Select(x => x.AsKernelFunction()).ToArray();

        var plugins = new KernelPluginCollection();
        plugins.AddFromFunctions(name, functions);

        // registered through factories so that the container owns disposal of the client
        builder.Container.Add((_, _) => client).AsKeyedSelf(name).Singleton();
        builder.Container.Add(_ => plugins).AsSelf().Singleton();

        return builder;
    }
}
