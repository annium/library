using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using XRest.Core.Extensions;
using XRest.Core.Helpers;
using XRest.Core.Models;

namespace XRest.Plugins.AspNetCore
{
    internal class ApiModelBuilder
    {
        private const string Controllers = "Controllers";
        private const string RouteArea = "area";
        private const string RouteController = "controller";
        private const string RouteDynamicKey = "dynamicKey";
        private const string BindingBody = "Body";

        public ApiModel Build(IReadOnlyCollection<ApiDescription> apiDescriptions)
        {
            var controllerActions = apiDescriptions
                .Select(x => x.ActionDescriptor)
                .OfType<ControllerActionDescriptor>()
                .GroupBy(x => x.ControllerTypeInfo)
                .ToArray();

            var controllers = new List<ControllerModel>();

            foreach (var item in controllerActions)
                controllers.Add(BuildControllerModel(item.ToArray()));

            return new ApiModel(controllers);
        }

        private ControllerModel BuildControllerModel(IReadOnlyCollection<ControllerActionDescriptor> actions)
        {
            var sample = actions.First();
            var nsParts = (sample.ControllerTypeInfo.Namespace ?? string.Empty)
                .ToNamespaceArray()
                .SkipWhile(x => x != Controllers)
                .Skip(1)
                .ToList();
            if (sample.RouteValues.TryGetValue(RouteArea, out var area) && !string.IsNullOrWhiteSpace(area))
                nsParts.Insert(0, area.PascalCase());
            if (sample.RouteValues.TryGetValue(RouteDynamicKey, out var dynamicKey) && !string.IsNullOrWhiteSpace(dynamicKey))
                nsParts.Add(dynamicKey.PascalCase());
            var ns = Namespace.New(nsParts);
            var name = sample.RouteValues[RouteController]!;
            var actionModels = actions
                .SelectMany(BuildActionModel)
                .ToArray();

            return new ControllerModel(ns, name, actionModels);
        }

        private IEnumerable<ActionModel> BuildActionModel(ControllerActionDescriptor action)
        {
            var methods = action.ActionConstraints!
                .OfType<HttpMethodActionConstraint>()
                .SelectMany(x => x.HttpMethods)
                .Select(x => new HttpMethod(x))
                .ToArray();

            var route = RouteHelper.NormalizeRoute(action.AttributeRouteInfo!.Template!);
            var routeParameters = RouteHelper.ParseRouteParameters(route);

            var parameters = action.Parameters
                .Where(x => x.BindingInfo?.BindingSource?.Id != BindingBody)
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
            var body = action.Parameters.SingleOrDefault(x => x.BindingInfo?.BindingSource?.Id == BindingBody)?.ParameterType;
            var response = action.MethodInfo.ReturnType;

            foreach (var method in methods)
                yield return new ActionModel(
                    action.ActionName,
                    method,
                    route,
                    parameters,
                    body,
                    response
                );
        }
    }
}