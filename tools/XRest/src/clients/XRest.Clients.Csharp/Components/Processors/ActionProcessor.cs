using System.Linq;
using Annium.Net.Types.Refs;
using XRest.Clients.Csharp.Views.Api;
using XRest.Core.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class ActionProcessor
{
    public static ActionView Process(ActionModel action, ProcessingContext ctx)
    {
        var name = action.Name;
        var path = action.Path;

        var pathParameters = action
            .Parameters.Where(x => x.Location == ParameterLocationEnum.Path)
            .Select(x => new ParameterView(RefProcessor.Process(x.Type, ctx), x.Name))
            .ToArray();

        var queryParameters = action
            .Parameters.Where(x => x.Location == ParameterLocationEnum.Query)
            .Select(x => new ParameterView(RefProcessor.Process(x.Type, ctx), x.Name))
            .ToArray();

        var body = action.Body is not null ? RefProcessor.Process(action.Body, ctx) : string.Empty;
        var response = ResolveResponseType(action.Response, ctx);

        return new ActionView(name, action.Method, path, pathParameters, queryParameters, body, response);
    }

    private static string ResolveResponseType(IRef? response, ProcessingContext ctx)
    {
        response = response is PromiseRef { Value: { } } promiseResponse ? promiseResponse.Value : response;

        return response is null ? string.Empty : RefProcessor.Process(response, ctx);
    }
}
