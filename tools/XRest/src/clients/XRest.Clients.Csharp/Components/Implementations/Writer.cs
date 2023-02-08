using System.IO;
using XRest.Clients.Dotnet.Views;
using XRest.Core.Components;
using XRest.Core.Extensions;
using XRest.Core.Models;
using static XRest.Clients.Dotnet.Helpers.WriterHelper;

namespace XRest.Clients.Dotnet.Components.Implementations;

internal class Writer : IWriter
{
    private readonly ITemplateWriter _templateWriter;

    public Writer(
        ITemplateWriter templateWriter
    )
    {
        _templateWriter = templateWriter;
    }

    public void Write(string output, ClientContainerView client, bool generateTestClient)
    {
        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        WriteIfMissing(output, "HttpRequestExtensions", "Templates.HttpRequestExtensions", new
        {
            Usages = new[] { "Annium.Net.Http" },
            client.Namespace,
        });

        if (generateTestClient)
            Write(output, "HttpResponseExtensions", "Templates.HttpResponseExtensions", new
            {
                Usages = new[]
                {
                    "System.Collections.Generic",
                    "System.Linq",
                    "System.Threading.Tasks",
                    "Annium.Data.Operations",
                    "Annium.Net.Http",
                },
                client.Namespace,
            });

        Write(output, "ClientBase", "Templates.ClientBase", new
        {
            Usages = new[] { "Annium.Net.Http" },
            client.Namespace,
        });

        WriteClientContainer(output, client.Namespace.ToNamespace(), client, generateTestClient);
    }

    private void WriteAbstractClient(string rootDir, Namespace rootNs, IClientView abstraction, bool generateTestClient)
    {
        switch (abstraction)
        {
            case ClientContainerView container:
                WriteClientContainer(rootDir, rootNs, container, generateTestClient);
                break;
            case ClientView client:
                WriteClient(rootDir, rootNs, client, generateTestClient);
                break;
        }
    }

    private void WriteClientContainer(string rootDir, Namespace rootNs, ClientContainerView container, bool generateTestClient)
    {
        var output = GetOutputPath(rootDir, rootNs, container.Namespace.ToNamespace());
        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        Write(output, container.Type, "Templates.ClientContainer", container);

        foreach (var client in container.Clients)
            WriteAbstractClient(rootDir, rootNs, client, generateTestClient);
    }

    private void WriteClient(string rootDir, Namespace rootNs, ClientView client, bool generateTestClient)
    {
        var output = GetOutputPath(rootDir, rootNs, client.Namespace.ToNamespace());
        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        Write(output, client.Type, generateTestClient ? "Templates.TestClient" : "Templates.Client", client);
    }

    private void WriteIfMissing<T>(string output, string fileName, string template, T data)
        where T : class
    {
        var path = Path.Combine(output, $"{fileName}.cs");
        if (!File.Exists(path))
            File.WriteAllText(path, _templateWriter.Write(template, data));
    }

    private void Write<T>(string output, string fileName, string template, T data)
        where T : class
    {
        File.WriteAllText(Path.Combine(output, $"{fileName}.cs"), _templateWriter.Write(template, data));
    }
}