using System.Linq;
using Annium.Net.Types.Refs;
using Annium.XRest.Clients.Csharp.Views.Api;
using Annium.XRest.Core.Models;

namespace Annium.XRest.Clients.Csharp.Components.Processors;

internal static class ActionProcessor
{
    public static ActionView Process(ActionModel action, string name, ProcessingContext ctx)
    {
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
        // the client awaits the call itself, so a promise contributes only what it resolves to — and a
        // promise resolving to nothing (a `Task`-returning action) leaves the call with no response at
        // all. Keeping the promise in that case rendered `Task<Task>`, plus a `Task` default-value
        // parameter, and the generated client did not compile
        if (response is PromiseRef promise)
            response = promise.Value;

        // a synchronous `void` action arrives as a plain void base type rather than a promise, and
        // rendering it produced `Task<Void>` plus a `Void defaultValue` parameter — CS0673
        if (response is BaseTypeRef { Name: BaseType.Void })
            response = null;

        return response is null ? string.Empty : RefProcessor.Process(response, ctx);
    }
}
