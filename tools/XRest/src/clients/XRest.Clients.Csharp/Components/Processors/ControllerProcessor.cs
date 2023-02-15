using System.Linq;
using Annium.Net.Types.Extensions;
using XRest.Clients.Csharp.Views;
using XRest.Core.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class ControllerProcessor
{
    public static ControllerView Process(ControllerModel controller, ApiContext apiCtx)
    {
        var clientNamespace = apiCtx.ClientsNamespace
            .Append(controller.Namespace.ToNamespace());
        var ctx = new ClientContext(clientNamespace, apiCtx.ModelsNamespace);
        var actions = controller.Actions
            .Select(x => ActionProcessor.Process(x, ctx))
            .ToArray();

        var usages = ctx.Usages;

        return new ControllerView(usages, clientNamespace, controller.Name, actions);
    }
}