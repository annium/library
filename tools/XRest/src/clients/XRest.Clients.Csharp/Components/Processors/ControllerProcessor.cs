using System.Linq;
using Annium.Net.Types.Extensions;
using XRest.Clients.Csharp.Views.Api;
using XRest.Core.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class ControllerProcessor
{
    public static ControllerView Process(ControllerModel controller, ApiContext apiCtx)
    {
        var ctx = new ProcessingContext(apiCtx.ModelsNamespace, apiCtx.Models);
        var actions = controller.Actions
            .Select(x => ActionProcessor.Process(x, ctx))
            .ToArray();

        var usages = ctx.Usages;
        var clientNamespace = apiCtx.ClientsNamespace
            .Append(controller.Namespace.ToNamespace());

        return new ControllerView(usages, clientNamespace, controller.Name, actions);
    }
}