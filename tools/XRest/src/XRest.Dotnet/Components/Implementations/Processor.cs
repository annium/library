using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Extensions.Primitives;
using XRest.Core.Models;
using XRest.Dotnet.Views;
using static XRest.Dotnet.Helpers.ProcessorHelper;

namespace XRest.Dotnet.Components.Implementations
{
    internal class Processor : IProcessor
    {
        private const string Areas = "Areas";
        private const string Client = "Client";
        private const string Clients = "Clients";

        public ClientContainerView Process(string ns, ApiModel api)
        {
            var clients = api.Controllers
                .GroupBy(x => (x.Area! ?? string.Empty).PascalCase())
                .ToDictionary(x => x.Key, x => x.Select(xx => ParseController(ns, xx)).ToArray());

            var containers = clients
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .Select(x => BuildContainerView(BuildNamespace(ns, Areas, x.Key!), x.Key!, $"{x.Key!}{Client}", x.Value))
                .ToArray();

            if (clients.TryGetValue(string.Empty, out var rootClients))
                return BuildContainerView(ns, Client, Client, containers.Concat<IClientView>(rootClients).ToArray());

            return BuildContainerView(ns, Client, Client, containers);
        }

        private ClientContainerView BuildContainerView(string ns, string name, string type, IReadOnlyCollection<IClientView> clients)
        {
            var usages = BuildUsages(ns, clients.Select(x => x.Namespace).Append("Annium.Net.Http"));

            return new ClientContainerView(usages, ns, name, type, clients);
        }

        private ClientView ParseController(string projectName, ControllerModel controller)
        {
            var references = controller.Actions
                .SelectMany(CollectReferences)
                .Distinct()
                .Where(x => !IsBaseType(x))
                .Select(x => x.Namespace!)
                .Append("Annium.Net.Http")
                .Append("System.Threading.Tasks")
                .ToArray();
            var ns = string.IsNullOrWhiteSpace(controller.Area)
                ? BuildNamespace(projectName, Clients)
                : BuildNamespace(projectName, Areas, controller.Area.PascalCase(), Clients);
            var usages = BuildUsages(ns, references);
            var actions = controller.Actions.Select(ParseAction).ToArray();

            return new ClientView(
                usages,
                ns,
                controller.Name,
                $"{controller.Name}{Client}",
                actions
            );
        }

        private IReadOnlyCollection<Type> CollectReferences(ActionModel action)
        {
            var references = new HashSet<Type>();

            foreach (var parameter in action.Parameters)
                CollectReferences(parameter.Type);

            if (action.Body != null)
                CollectReferences(action.Body);

            if (action.Response != null)
                CollectReferences(action.Response);

            return references;

            void CollectReferences(Type type)
            {
                references.Add(type);

                if (type.IsGenericType)
                    foreach (var argument in type.GetGenericArguments())
                        CollectReferences(argument);
            }
        }


        private ActionView ParseAction(ActionModel action)
        {
            var pathParameters = action.Parameters
                .Where(x => x.Location == ParameterLocationEnum.Path)
                .Select(x => new ParameterView(x.Name, x.Type.FriendlyName()))
                .ToArray();

            var queryParameters = action.Parameters
                .Where(x => x.Location == ParameterLocationEnum.Query)
                .Select(x => new ParameterView(x.Name, x.Type.FriendlyName()))
                .ToArray();

            return new ActionView(
                action.Name,
                action.Method,
                action.Path,
                pathParameters,
                queryParameters,
                action.Body?.FriendlyName(),
                ResolveResponseType(action.Response)?.FriendlyName()
            );

            static Type? ResolveResponseType(Type? type)
            {
                if (type is null || type == typeof(Task))
                    return null;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                    return type.GetGenericArguments()[0];

                return type;
            }
        }
    }
}