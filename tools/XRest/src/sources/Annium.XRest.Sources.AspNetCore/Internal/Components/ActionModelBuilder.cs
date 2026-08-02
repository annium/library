using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Annium.XRest.Core.Models;
using Annium.XRest.Sources.AspNetCore.Internal.Helpers;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Namotion.Reflection;
using ParseHelper = Annium.XRest.Sources.Shared.Helpers.ParseHelper;

namespace Annium.XRest.Sources.AspNetCore.Internal.Components;

internal static class ActionModelBuilder
{
    public static IEnumerable<ActionModel> Build(ControllerActionDescriptor action, MappingContext ctx)
    {
        // an action without an HTTP method constraint (a bare [Route]) answers every verb — there is
        // no single method to emit, and ActionConstraints is null rather than empty in that case
        var methods = (action.ActionConstraints ?? [])
            .OfType<HttpMethodActionConstraint>()
            .SelectMany(x => x.HttpMethods)
            .Select(x => new HttpMethod(x))
            .ToArray();
        if (methods.Length == 0)
            yield break;

        // conventionally routed actions carry no attribute route to describe
        if (action.AttributeRouteInfo?.Template is not { } template)
            yield break;

        var parameterNames = action.Parameters.Select(x => x.Name).ToArray();
        var route = RouteHelper.NormalizeRoute(template, parameterNames);
        var routeParameters = RouteHelper.ParseRouteParameters(template);

        var parameters = action
            .Parameters.Where(x => x.BindingInfo?.BindingSource?.Id != Constants.BindingBody)
            .SelectMany(x => BuildParameterModels(x, routeParameters, ctx))
            .ToArray();
        var body = action
            .Parameters.SingleOrDefault(x => x.BindingInfo?.BindingSource?.Id == Constants.BindingBody)
            ?.ParameterType;
        var response = action.MethodInfo.ReturnType;

        foreach (var method in methods)
            yield return new ActionModel(
                method,
                route,
                action.ActionName,
                parameters,
                body is not null ? ctx.Map(body.ToContextualType()) : null,
                ctx.Map(response.ToContextualType())
            );
    }

    private static IEnumerable<ParameterModel> BuildParameterModels(
        ParameterDescriptor param,
        IReadOnlyCollection<string> routeParameters,
        MappingContext ctx
    )
    {
        if (ParseHelper.IsSkippedType(param.ParameterType))
            return [];

        // route matching is case-insensitive, so `{Id}` binds a parameter declared as `id`
        if (routeParameters.Contains(param.Name, StringComparer.OrdinalIgnoreCase))
            return
            [
                new ParameterModel(
                    ParameterLocationEnum.Path,
                    ctx.Map(param.ParameterType.ToContextualType()),
                    param.Name
                ),
            ];

        if (ParseHelper.IsAllowedQueryType(param.ParameterType, ctx.Config))
            return
            [
                new ParameterModel(
                    ParameterLocationEnum.Query,
                    ctx.Map(param.ParameterType.ToContextualType()),
                    param.Name
                ),
            ];

        return param
            .ParameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
            .Where(p => p.CanRead)
            .Select(p => new ParameterModel(
                ParameterLocationEnum.Query,
                ctx.Map(p.ToContextualProperty().PropertyType),
                p.Name.CamelCase()
            ));
    }
}
