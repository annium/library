using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.XRest.Clients.Csharp.Views.Api;
using Annium.XRest.Core.Models;

namespace Annium.XRest.Clients.Csharp.Components.Processors;

internal static class ControllerProcessor
{
    public static ControllerView Process(ControllerModel controller, ApiContext apiCtx)
    {
        var ctx = new ProcessingContext(apiCtx.ModelsNamespace, apiCtx.Models) { ReservedNames = apiCtx.ReservedNames };

        // one action answering several verbs — `[HttpGet]` and `[HttpDelete]` on the same method —
        // arrives as one model per verb, all named after that method. The generated client would then
        // declare the same method twice, so the verb disambiguates them; names stay as they were
        // wherever there is nothing to disambiguate
        var ambiguous = controller
            .Actions.GroupBy(x => x.Name)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToHashSet();
        var takenNames = new HashSet<string>();
        var actions = controller
            .Actions.Select(x =>
                ActionProcessor.Process(
                    x,
                    Naming.Take(
                        ambiguous.Contains(x.Name) ? $"{x.Method.Method.PascalCase()}{x.Name}" : x.Name,
                        takenNames
                    ),
                    ctx
                )
            )
            .ToArray();

        var usages = ctx.Usages;
        var clientNamespace = apiCtx.ClientsNamespace.Append(controller.Namespace.ToNamespace());

        return new ControllerView(usages, clientNamespace, controller.Name, actions);
    }
}
