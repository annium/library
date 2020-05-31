using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Primitives;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using XRest.Core.Helpers;
using XRest.Core.Infrastructure.JsonConverters;
using XRest.Core.Models;

namespace XRest.AspNetCore
{
    public class XRestMiddleware
    {
        private const string RouteArea = "area";
        private const string RouteController = "controller";

        private const string BindingBody = "Body";

        private readonly RequestDelegate _next;
        private readonly IApiDescriptionGroupCollectionProvider _descriptionProvider;

        private readonly ISerializer<string> _serializer = StringSerializer.Configure(opts =>
        {
            opts.Converters.Add(new TypeJsonConverter());
            opts.WriteIndented = true;
            opts.ConfigureDefault();
        });

        public XRestMiddleware(
            RequestDelegate next,
            IApiDescriptionGroupCollectionProvider descriptionProvider
        )
        {
            _next = next;
            _descriptionProvider = descriptionProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/.xrest"))
            {
                await _next(context);
                return;
            }

            var apiModel = BuildApiModel();
            await context.Response.WriteAsync(_serializer.Serialize(apiModel));
        }

        private ApiModel BuildApiModel()
        {
            var controllerActions = _descriptionProvider.ApiDescriptionGroups.Items
                .SelectMany(x => x.Items)
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
            sample.RouteValues.TryGetValue(RouteArea, out var area);
            var name = sample.RouteValues[RouteController];
            var actionModels = actions
                .SelectMany(BuildActionModel)
                .ToArray();

            return new ControllerModel(area, name, actionModels);
        }

        private IEnumerable<ActionModel> BuildActionModel(ControllerActionDescriptor action)
        {
            var methods = action.ActionConstraints
                .OfType<HttpMethodActionConstraint>()
                .SelectMany(x => x.HttpMethods)
                .Select(x => new HttpMethod(x))
                .ToArray();

            var route = action.AttributeRouteInfo.Template;
            var routeParameters = RouteHelper.ParseRouteParameters(route);

            var parameters = action.Parameters
                .Where(x => x.BindingInfo?.BindingSource.Id != BindingBody)
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
            var body = action.Parameters.SingleOrDefault(x => x.BindingInfo?.BindingSource.Id == BindingBody)?.ParameterType;
            var response = action.MethodInfo.ReturnType;

            foreach (var method in methods)
                yield return new ActionModel(
                    action.ActionName,
                    method,
                    route,
                    parameters,
                    body,
                    response,
                    new AuthModel(false)
                );
        }
    }
}