using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.XRest.Clients.Csharp.Views.Api;
using Annium.XRest.Core.Models;

namespace Annium.XRest.Clients.Csharp.Components.Processors;

internal static class Processor
{
    public static ApiView Process(Namespace rootNamespace, ApiModel api)
    {
        var apiCtx = new ApiContext(
            rootNamespace.Append(Constants.ClientsNamespace.ToNamespace()),
            rootNamespace.Append(Constants.ModelsNamespace.ToNamespace()),
            api.Models
        );
        var controllers = api.Controllers.Select(x => ControllerProcessor.Process(x, apiCtx)).ToArray();

        var client = ClientBuilder.BuildClient(apiCtx.ClientsNamespace, "Root", "Root", controllers);

        var models = api
            .Models.Select(x => ModelProcessor.Process(x, new ProcessingContext(apiCtx.ModelsNamespace, api.Models)))
            .ToArray();

        return new ApiView(rootNamespace, client, models);
    }
}
