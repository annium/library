using System.IO;
using XRest.Clients.Csharp.Views;

namespace XRest.Clients.Csharp.Components.Writers;

internal class Writer
{
    private readonly ClientWriter _clientWriter;
    private readonly ModelWriter _modelWriter;

    public Writer(
        ClientWriter clientWriter,
        ModelWriter modelWriter
    )
    {
        _clientWriter = clientWriter;
        _modelWriter = modelWriter;
    }

    public void Write(string output, ApiView api, bool generateTestClient)
    {
        _clientWriter.Write(Path.Combine(output, Constants.ClientsNamespace), api.Client, generateTestClient);
    }
}