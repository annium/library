using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using XRest.Clients.Csharp.Views;
using XRest.Core.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class Processor
{
    public static ApiView Process(Namespace rootNamespace, ApiModel api)
    {
        var apiCtx = new ApiContext(
            rootNamespace.Append(Constants.ClientsNamespace.ToNamespace()),
            rootNamespace.Append(Constants.ModelsNamespace.ToNamespace()),
            api.Models
        );
        var controllers = api.Controllers
            .Select(x => ControllerProcessor.Process(x, apiCtx))
            .ToArray();

        var client = ClientBuilder.BuildClient(apiCtx.ClientsNamespace, "Root", "Root", controllers);

        var models = api.Models
            .Select(x => ModelProcessor.Process(x, new ProcessingContext(apiCtx.ModelsNamespace)))
            .ToArray();

        return new ApiView(rootNamespace, client, models);
    }
}