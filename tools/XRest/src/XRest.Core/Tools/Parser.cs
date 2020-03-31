using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using XRest.Core.Helpers;
using XRest.Core.Models;

namespace XRest.Core.Tools
{
    public class Parser
    {
        public ApiModel Parse(IReadOnlyCollection<Type> controllerTypes)
        {
            var controllerModels = controllerTypes.Select(ParseController).ToArray();

            return new ApiModel(controllerModels);
        }

        public ControllerModel ParseController(
            Type controllerType
        )
        {
            var controllerName = controllerType.Name.Replace("Controller", string.Empty);
            var controllerAuth = ParseAuth(controllerType.GetCustomAttributes());
            var controllerRoute = controllerType.GetCustomAttribute<RouteAttribute>()?.Template;

            var actions = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(ParseHelper.IsAction).ToArray();
            var actionModels = actions.Select(x => ParseAction(controllerName, controllerAuth, controllerRoute, x)).ToArray();

            return new ControllerModel(controllerName, actionModels);
        }

        private ActionModel ParseAction(
            string controllerName,
            AuthModel? controllerAuth,
            string? controllerRoute,
            MethodInfo action
        )
        {
            var actionName = action.Name;
            var methodAttribute = action.GetCustomAttributes<HttpMethodAttribute>().FirstOrDefault();
            var method = new HttpMethod(methodAttribute?.HttpMethods.FirstOrDefault() ?? HttpMethod.Get.Method);
            var route = RouteHelper.BuildRoute(controllerName, controllerRoute, action.Name, methodAttribute?.Template);

            var routeParameters = RouteHelper.ParseRouteParameters(route);

            var parameters = action.GetParameters()
                .Where(x => x.GetCustomAttribute<FromBodyAttribute>() is null)
                .Select(x => new ParameterModel(
                    x.Name!,
                    routeParameters.Contains(x.Name)
                        ? ParameterLocationEnum.Path
                        : ParameterLocationEnum.Query,
                    x.ParameterType
                ))
                .OrderBy(x => x.Name)
                .ToArray();

            var bodyType = action.GetParameters()
                .SingleOrDefault(x => x.GetCustomAttribute<FromBodyAttribute>() != null)?
                .ParameterType;

            var auth = controllerAuth ?? ParseAuth(action.GetCustomAttributes()) ?? new AuthModel(false);

            return new ActionModel(actionName, method, route, parameters, bodyType, auth);
        }

        private AuthModel? ParseAuth(IEnumerable<Attribute> attributes)
        {
            var attribute = attributes.FirstOrDefault(x => x.GetType().Name == nameof(AuthorizeAttribute));

            return attribute is null ? null : new AuthModel(true);
        }
    }
}