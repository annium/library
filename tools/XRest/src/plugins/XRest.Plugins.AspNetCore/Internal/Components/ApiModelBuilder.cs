using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using XRest.Core.Models;

namespace XRest.Plugins.AspNetCore.Internal.Components;

internal static class ApiModelBuilder
{
    public static ApiModel Build(IReadOnlyCollection<ApiDescription> apiDescriptions)
    {
        var controllerActions = apiDescriptions
            .Select(x => x.ActionDescriptor)
            .OfType<ControllerActionDescriptor>()
            .GroupBy(x => x.ControllerTypeInfo)
            .ToArray();

        var controllers = controllerActions
            .Select(item => ControllerModelBuilder.Build(item.ToArray()))
            .ToArray();

        return new ApiModel(controllers);
    }
}