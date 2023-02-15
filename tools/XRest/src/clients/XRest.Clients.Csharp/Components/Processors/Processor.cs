using System;
using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using XRest.Clients.Csharp.Views;
using XRest.Core.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class Processor
{
    public static ClientContainerView Process(Namespace rootNamespace, ApiModel api)
    {
        var apiCtx = new ApiContext(rootNamespace, rootNamespace.Append(Constants.ModelsNamespace.ToNamespace()), api.Models);
        var controllers = api.Controllers
            .Select(x => ControllerProcessor.Process(x, apiCtx))
            .ToArray();

        var usages = Array.Empty<string>();
        var @namespace = string.Empty;
        var name = "X";
        var type = "X";
        var clients = Array.Empty<IClientView>();

        return new ClientContainerView(usages, @namespace, name, type, clients);
    }
}