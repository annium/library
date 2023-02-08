using System.Collections.Generic;
using System.Linq;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Mvc.Controllers;
using XRest.Core.Extensions;
using XRest.Core.Models;

namespace XRest.Plugins.AspNetCore.Internal.Components;

internal static class ControllerModelBuilder
{
    public static ControllerModel Build(IReadOnlyCollection<ControllerActionDescriptor> actions)
    {
        var sample = actions.First();
        var nsParts = (sample.ControllerTypeInfo.Namespace ?? string.Empty)
            .ToNamespaceArray()
            .SkipWhile(x => x != Constants.Controllers)
            .Skip(1)
            .ToList();
        if (sample.RouteValues.TryGetValue(Constants.RouteArea, out var area) && !string.IsNullOrWhiteSpace(area))
            nsParts.Insert(0, area.PascalCase());
        if (sample.RouteValues.TryGetValue(Constants.RouteDynamicKey, out var dynamicKey) && !string.IsNullOrWhiteSpace(dynamicKey))
            nsParts.Add(dynamicKey.PascalCase());
        var ns = Namespace.New(nsParts);
        var name = sample.RouteValues[Constants.RouteController]!;
        var actionModels = actions
            .SelectMany(ActionModelBuilder.Build)
            .ToArray();

        return new ControllerModel(ns, name, actionModels);
    }
}