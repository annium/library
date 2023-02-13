using System;
using System.Collections.Generic;
using Annium.Net.Types.Models;
using XRest.Clients.Csharp.Views;
using XRest.Core.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class ActionProcessor
{
    public static ActionView Process(ActionModel action, IReadOnlyCollection<ModelBase> models)
    {
        var name = action.Name;
        var path = action.Path;
        var pathParameters = Array.Empty<ParameterView>();
        var queryParameters = Array.Empty<ParameterView>();
        var body = string.Empty;
        var response = string.Empty;
        var responseDefault = string.Empty;

        return new ActionView(name, action.Method, path, pathParameters, queryParameters, body, response, responseDefault);
    }
}