using System.IO;
using Annium.Net.Types.Extensions;
using Annium.XRest.Clients.Csharp.Views.Api;

namespace Annium.XRest.Clients.Csharp.Components.Writers;

internal class Writer
{
    private readonly ClientWriter _clientWriter;
    private readonly ModelWriter _modelWriter;

    public Writer(ClientWriter clientWriter, ModelWriter modelWriter)
    {
        _clientWriter = clientWriter;
        _modelWriter = modelWriter;
    }

    public void Write(string output, ApiView api, bool generateTestClient)
    {
        _clientWriter.Write(Path.Combine(output, Constants.ClientsNamespace), api.Client, generateTestClient);
        _modelWriter.Write(
            Path.Combine(output, Constants.ModelsNamespace),
            api.Namespace.Append(Constants.ModelsNamespace),
            api.Models
        );
    }
}
