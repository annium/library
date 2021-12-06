using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using XRest.Core.Extensions;
using XRest.Core.Helpers;
using XRest.Core.Models;

namespace XRest.Sources.Assembly.Components.Internal;

internal class Parser : IParser
{
    private const string Controller = "Controller";
    private const string Controllers = "Controllers";

    public ApiModel Parse(IReadOnlyCollection<Type> controllerTypes)
    {
        var controllerModels = controllerTypes.Select(ParseController).ToArray();

        return new ApiModel(controllerModels);
    }

    private ControllerModel ParseController(
        Type controllerType
    )
    {
        var nsParts = (controllerType.Namespace ?? string.Empty)
            .ToNamespaceArray()
            .SkipWhile(x => x != Controllers)
            .Skip(1)
            .ToList();
        var area = controllerType.GetCustomAttribute<AreaAttribute>()?.RouteValue;
        if (area is not null)
            nsParts.Insert(0, area.PascalCase());
        var ns = Namespace.New(nsParts);
        var name = controllerType.Name.Replace(Controller, string.Empty);
        var route = controllerType.GetCustomAttribute<RouteAttribute>()?.Template;

        var actions = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(ParseHelper.IsAction).ToArray();
        var actionModels = actions.Select(x => ParseAction(area, name, route, x)).ToArray();

        return new ControllerModel(ns, name, actionModels);
    }

    private ActionModel ParseAction(
        string? controllerArea,
        string controllerName,
        string? controllerRoute,
        MethodInfo action
    )
    {
        var actionName = action.Name;
        var methodAttribute = action.GetCustomAttributes<HttpMethodAttribute>().FirstOrDefault();
        var method = new HttpMethod(methodAttribute?.HttpMethods.FirstOrDefault() ?? HttpMethod.Get.Method);
        var route = RouteHelper.BuildRoute(controllerArea, controllerName, controllerRoute, action.Name, methodAttribute?.Template);

        var routeParameters = RouteHelper.ParseRouteParameters(route);

        var parameters = action.GetParameters()
            .Where(x => x.GetCustomAttribute<FromBodyAttribute>() is null)
            .SelectMany(x =>
            {
                if (routeParameters.Contains(x.Name))
                    return new[] { new ParameterModel(x.Name!, ParameterLocationEnum.Path, x.ParameterType) };

                if (ParseHelper.IsAllowedQueryType(x.ParameterType))
                    return new[] { new ParameterModel(x.Name!, ParameterLocationEnum.Query, x.ParameterType) };

                return x.ParameterType
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    .Where(p => p.CanRead)
                    .Select(p => new ParameterModel(
                        p.Name.CamelCase(),
                        ParameterLocationEnum.Query,
                        p.PropertyType
                    ));
            })
            .OrderBy(x => x.Name)
            .ToArray();

        var bodyType = action.GetParameters()
            .SingleOrDefault(x => x.GetCustomAttribute<FromBodyAttribute>() != null)?
            .ParameterType;

        var responseType = action.ReturnType == typeof(void) ? null : action.ReturnType;

        return new ActionModel(actionName, method, route, parameters, bodyType, responseType);
    }
}