using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Annium.Core.Primitives;
using Annium.Net.Types;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Namotion.Reflection;
using XRest.Core.Helpers;
using XRest.Core.Models;

namespace XRest.Plugins.AspNetCore.Internal.Components;

internal static class ActionModelBuilder
{
    public static IEnumerable<ActionModel> Build(ControllerActionDescriptor action, IModelMapper modelMapper)
    {
        var methods = action.ActionConstraints!
            .OfType<HttpMethodActionConstraint>()
            .SelectMany(x => x.HttpMethods)
            .Select(x => new HttpMethod(x))
            .ToArray();

        var route = RouteHelper.NormalizeRoute(action.AttributeRouteInfo!.Template!);
        var routeParameters = RouteHelper.ParseRouteParameters(route);

        var parameters = action.Parameters
            .Where(x => x.BindingInfo?.BindingSource?.Id != Constants.BindingBody)
            .SelectMany(x => BuildParameterModels(x, routeParameters, modelMapper))
            .ToArray();
        var body = action.Parameters.SingleOrDefault(x => x.BindingInfo?.BindingSource?.Id == Constants.BindingBody)?.ParameterType;
        var response = action.MethodInfo.ReturnType;

        foreach (var method in methods)
            yield return new ActionModel(
                method,
                route,
                action.ActionName,
                parameters,
                body is not null ? modelMapper.Map(body.ToContextualType()) : null,
                modelMapper.Map(response.ToContextualType())
            );
    }

    private static IEnumerable<ParameterModel> BuildParameterModels(ParameterDescriptor param, IReadOnlyCollection<string> routeParameters, IModelMapper modelMapper)
    {
        if (ParseHelper.IsSkippedType(param.ParameterType))
            return Array.Empty<ParameterModel>();

        if (routeParameters.Contains(param.Name))
            return new[] { new ParameterModel(ParameterLocationEnum.Path, modelMapper.Map(param.ParameterType.ToContextualType()), param.Name) };

        if (ParseHelper.IsAllowedQueryType(param.ParameterType))
            return new[] { new ParameterModel(ParameterLocationEnum.Query, modelMapper.Map(param.ParameterType.ToContextualType()), param.Name) };

        return param.ParameterType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
            .Where(p => p.CanRead)
            .Select(p => new ParameterModel(
                ParameterLocationEnum.Query,
                modelMapper.Map(p.ToContextualProperty().PropertyType),
                p.Name.CamelCase()
            ));
    }
}