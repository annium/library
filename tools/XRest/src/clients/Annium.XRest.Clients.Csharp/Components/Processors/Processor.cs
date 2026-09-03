using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.XRest.Clients.Csharp.Views.Api;
using Annium.XRest.Core.Models;

namespace Annium.XRest.Clients.Csharp.Components.Processors;

internal static class Processor
{
    private const string RootName = "Root";

    public static ApiView Process(Namespace rootNamespace, ApiModel api)
    {
        var clientsNamespace = rootNamespace.Append(Constants.ClientsNamespace.ToNamespace());

        // the container types are generated into the same namespaces the clients live in, so a model
        // sharing one of their names has to be referenced in full — collected before any reference is
        // written, since the containers themselves are only built afterwards
        var reservedNames = ClientBuilder
            .GetBranchNames(
                clientsNamespace,
                api.Controllers.Select(x => clientsNamespace.Append(x.Namespace.ToString().ToNamespace()))
            )
            .Values.Append(RootName)
            // …and the leaf client of each controller, declared in the very file its own actions are
            // written into, so a model sharing that name binds to the client wrapper instead
            .Concat(api.Controllers.Select(x => $"{x.Name}Client"))
            .ToHashSet();

        var apiCtx = new ApiContext(
            clientsNamespace,
            rootNamespace.Append(Constants.ModelsNamespace.ToNamespace()),
            api.Models
        )
        {
            ReservedNames = reservedNames,
        };
        var controllers = api.Controllers.Select(x => ControllerProcessor.Process(x, apiCtx)).ToArray();

        var client = ClientBuilder.BuildClient(apiCtx.ClientsNamespace, RootName, RootName, controllers);

        var models = api
            .Models.Select(x =>
                ModelProcessor.Process(
                    x,
                    new ProcessingContext(apiCtx.ModelsNamespace, api.Models) { ReservedNames = reservedNames }
                )
            )
            .ToArray();

        return new ApiView(rootNamespace, client, models);
    }
}
