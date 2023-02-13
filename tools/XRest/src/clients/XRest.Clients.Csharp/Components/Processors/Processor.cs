using System;
using System.Linq;
using Annium.Net.Types.Models;
using XRest.Clients.Csharp.Views;
using XRest.Core.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class Processor
{
    public static ClientContainerView Process(Namespace ns, ApiModel api)
    {
        var controllers = api.Controllers
            .Select(x => ControllerProcessor.Process(x, api.Models))
            .ToArray();

        var usages = Array.Empty<string>();
        var @namespace = string.Empty;
        var name = "X";
        var type = "X";
        var clients = Array.Empty<IClientView>();

        return new ClientContainerView(usages, @namespace, name, type, clients);
    }
}