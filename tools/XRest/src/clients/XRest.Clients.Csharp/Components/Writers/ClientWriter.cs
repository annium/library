using System.IO;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using XRest.Clients.Csharp.Views.Client;
using static XRest.Clients.Csharp.Components.Writers.WriterHelper;

namespace XRest.Clients.Csharp.Components.Writers;

internal class ClientWriter
{
    private readonly FileWriter _writer;

    public ClientWriter(FileWriter writer)
    {
        _writer = writer;
    }

    public void Write(string output, IClientView client, bool generateTestClient)
    {
        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        _writer.TryWrite(
            output,
            "HttpRequestExtensions",
            "Templates.HttpRequestExtensions",
            new { Usages = new[] { Constants.AnniumNetHttpNamespace }, client.Namespace }
        );

        if (generateTestClient)
            _writer.Write(
                output,
                "HttpResponseExtensions",
                "Templates.HttpResponseExtensions",
                new
                {
                    Usages = new[]
                    {
                        "System.Collections.Generic",
                        "System.Linq",
                        "System.Threading.Tasks",
                        Constants.AnniumDataOperationsNamespace,
                        Constants.AnniumNetHttpNamespace,
                    },
                    client.Namespace,
                }
            );

        WriteAbstractClient(output, client.Namespace.ToNamespace(), client, generateTestClient);
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

    private void WriteClientContainer(
        string rootDir,
        Namespace rootNs,
        ClientContainerView container,
        bool generateTestClient
    )
    {
        var output = GetOutputPath(rootDir, rootNs, container.Namespace.ToNamespace());
        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        _writer.Write(output, container.Type, "Templates.ClientContainer", container);

        foreach (var client in container.Clients)
            WriteAbstractClient(rootDir, rootNs, client, generateTestClient);
    }

    private void WriteClient(string rootDir, Namespace rootNs, ClientView client, bool generateTestClient)
    {
        var output = GetOutputPath(rootDir, rootNs, client.Namespace.ToNamespace());
        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        _writer.Write(output, client.Type, generateTestClient ? "Templates.TestClient" : "Templates.Client", client);
    }
}
