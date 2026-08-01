using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

namespace Annium.Integrations.AI.SemanticKernel;

public static class SemanticKernelBuilderBaseExtensions
{
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

    public static ISemanticKernelBuilder WithMcpFunctionsFromSseServer(
        this ISemanticKernelBuilder builder,
        string name,
        string url
    )
    {
#pragma warning disable VSTHRD002
        builder.Container.Add((_, _) => CreateMcpClientAsync(name, url).Result).AsKeyedSelf(name).Singleton();
        builder
            .Container.Add(sp =>
            {
                var client = sp.ResolveKeyed<McpClient>(name);
                var functions = LoadFunctionsFromServerAsync(client).Result;
                var plugins = new KernelPluginCollection();
                plugins.AddFromFunctions(name, functions);

                return plugins;
            })
            .AsSelf()
            .Singleton();
#pragma warning restore VSTHRD002

        return builder;

        static async Task<McpClient> CreateMcpClientAsync(string name, string url)
        {
            var client = await McpClient.CreateAsync(
                new HttpClientTransport(
                    new HttpClientTransportOptions
                    {
                        Name = name,
                        Endpoint = new Uri(url),
                        TransportMode = HttpTransportMode.StreamableHttp,
                    }
                )
            );

            return client;
        }

        static async Task<IReadOnlyCollection<KernelFunction>> LoadFunctionsFromServerAsync(McpClient client)
        {
            var tools = await client.ListToolsAsync();
            var functions = tools.Select(x => x.AsKernelFunction()).ToArray();

            return functions;
        }
    }
}
