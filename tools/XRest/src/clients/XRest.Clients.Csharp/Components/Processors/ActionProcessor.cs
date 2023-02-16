using System.Linq;
using XRest.Clients.Csharp.Views;
using XRest.Core.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class ActionProcessor
{
    public static ActionView Process(ActionModel action, ProcessingContext ctx)
    {
        var name = action.Name;
        var path = action.Path;

        var pathParameters = action.Parameters
            .Where(x => x.Location == ParameterLocationEnum.Path)
            .Select(x => new ParameterView(RefProcessor.Process(x.Type, ctx), x.Name))
            .ToArray();

        var queryParameters = action.Parameters
            .Where(x => x.Location == ParameterLocationEnum.Query)
            .Select(x => new ParameterView(RefProcessor.Process(x.Type, ctx), x.Name))
            .ToArray();

        var body = action.Body is not null ? RefProcessor.Process(action.Body, ctx) : string.Empty;
        var (response, responseDefault) = ResponseProcessor.Resolve(action.Response, ctx);

        return new ActionView(name, action.Method, path, pathParameters, queryParameters, body, response, responseDefault);
    }
}