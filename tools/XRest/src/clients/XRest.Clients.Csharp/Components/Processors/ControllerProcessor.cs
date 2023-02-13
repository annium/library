using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using XRest.Clients.Csharp.Views;
using XRest.Core.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class ControllerProcessor
{
    public static ControllerView Process(ControllerModel controller, IReadOnlyCollection<ModelBase> models)
    {
        var actions = controller.Actions
            .Select(x => ActionProcessor.Process(x, models))
            .ToArray();

        var namespaces = Array.Empty<Namespace>();
        var @namespace = controller.Namespace.ToNamespace();

        return new ControllerView(namespaces, @namespace, string.Empty, string.Empty, actions);
    }
}