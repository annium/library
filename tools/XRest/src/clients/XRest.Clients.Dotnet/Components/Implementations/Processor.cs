using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Data.Operations;
using Annium.Core.Primitives;
using XRest.Clients.Dotnet.Extensions;
using XRest.Clients.Dotnet.Views;
using XRest.Core.Extensions;
using XRest.Core.Models;
using static XRest.Clients.Dotnet.Helpers.ProcessorHelper;

namespace XRest.Clients.Dotnet.Components.Implementations
{
    internal class Processor : IProcessor
    {
        private static readonly Namespace AnniumNetHttp = Namespace.New("Annium.Net.Http");
        private const string Client = "Client";
        private const string Root = "Root";

        public ClientContainerView Process(Namespace ns, ApiModel api)
        {
            var tm = TypeManager.GetInstance(api.Assembly, false);
            var candidates = api.Controllers
                .Select(x => ParseController(ns, x, tm))
                .ToArray();

            var node = BuildClientNode(ns, Client, Root, candidates);

            return node;
            // var containers = clients
            //     .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            //     .Select(x => BuildContainerView(BuildNamespace(ns, x.Key!), x.Key!, $"{x.Key!}{Client}", x.Value))
            //     .ToArray();
            //
            // if (clients.TryGetValue(string.Empty, out var rootClients))
            //     return BuildContainerView(ns, Client, Client, containers.Concat<IClientView>(rootClients).ToArray());
            //
            // return BuildContainerView(ns, Client, Client, containers);
        }

        private ClientContainerView BuildClientNode(Namespace ns, string name, string type, IReadOnlyCollection<ClientCandidate> candidates)
        {
            if (candidates.Count == 0)
                throw new ArgumentException("Can't build container without clients");

            if (candidates.Count == 1)
                return new ClientContainerView(
                    new[] { candidates.First().Namespace, AnniumNetHttp }.ToUsagesFrom(ns).ToUsageStrings(),
                    ns.ToString(),
                    name,
                    type,
                    candidates.Select(x => (ClientView) x).ToArray()
                );

            var lookup = candidates.ToLookup(x => x.Namespace == ns);

            var children = lookup[true].ToArray();

            var ancestors = lookup[false]
                .GroupBy(x => ns.Append(x.Namespace.From(ns).First()).ToNamespace())
                .ToDictionary(
                    x => x.Key,
                    x => BuildClientNode(x.Key, x.Key.Last(), $"{x.Key.Last()}{Root}", x.ToArray())
                );

            var usages = children
                .Select(x => x.Namespace)
                .Append(AnniumNetHttp)
                .Concat(ancestors.Keys)
                .ToUsagesFrom(ns)
                .ToUsageStrings();

            var clients = ancestors.Values
                .OrderBy(x => x.Namespace.ToString())
                .Concat<IClientView>(children.Select(x => (ClientView) x).OrderBy(x => x.Namespace))
                .ToArray();

            return new ClientContainerView(usages, ns.ToString(), name, type, clients);
        }
        //
        // private ClientContainerView BuildContainerView(string ns, string name, string type, IReadOnlyCollection<IClientView> clients)
        // {
        //     var usages = BuildUsages(ns, clients.Select(x => x.Namespace).Append("Annium.Net.Http"));
        //
        //     return new ClientContainerView(usages, ns, name, type, clients);
        // }

        private ClientCandidate ParseController(Namespace rootNs, ControllerModel controller, ITypeManager tm)
        {
            var ns = rootNs.Concat(controller.Namespace).ToNamespace();
            var usages = controller.Actions
                .SelectMany(x => CollectReferences(x, tm))
                .Distinct()
                .Where(x => !IsBaseType(x))
                .Select(x => x.Namespace!)
                .Append("Annium.Net.Http")
                .Append("System.Threading.Tasks")
                .Select(Namespace.New)
                .ToUsagesFrom(ns);
            var actions = controller.Actions.Select(x => ParseAction(x, tm)).ToArray();

            return new ClientCandidate(
                usages,
                ns,
                controller.Name,
                $"{controller.Name}{Client}",
                actions
            );
        }

        private IReadOnlyCollection<Type> CollectReferences(ActionModel action, ITypeManager tm)
        {
            var references = new HashSet<Type>();

            foreach (var parameter in action.Parameters)
                CollectTypeReferences(parameter.Type);

            if (action.Body != null)
                CollectTypeReferences(action.Body);

            if (action.Response != null)
            {
                CollectTypeReferences(action.Response);
                var response = ResolveResponseType(action.Response);
                var (_, responseType) = ResolveResponseKindAndInnerType(response);
                var defaultType = ResolveTypeDefaultType(responseType, tm);
                if (defaultType != null)
                    CollectTypeReferences(action.Response);
            }

            return references;

            void CollectTypeReferences(Type type)
            {
                references.Add(type);

                if (type.IsGenericType)
                    foreach (var argument in type.GetGenericArguments())
                        CollectTypeReferences(argument);
            }
        }


        private ActionView ParseAction(ActionModel action, ITypeManager tm)
        {
            var pathParameters = action.Parameters
                .Where(x => x.Location == ParameterLocationEnum.Path)
                .Select(x => new ParameterView(x.Name, x.Type.FriendlyName()))
                .ToArray();

            var queryParameters = action.Parameters
                .Where(x => x.Location == ParameterLocationEnum.Query)
                .Select(x => new ParameterView(x.Name, x.Type.FriendlyName()))
                .ToArray();

            var response = ResolveResponseType(action.Response);
            var (kind, responseType) = ResolveResponseKindAndInnerType(response);
            var defaultType = ResolveTypeDefaultType(responseType, tm);
            var responseDefault = GetDefaultExpression(kind, responseType, defaultType);

            return new ActionView(
                action.Name,
                action.Method,
                action.Path,
                pathParameters,
                queryParameters,
                action.Body?.FriendlyName() ?? string.Empty,
                response?.FriendlyName() ?? string.Empty,
                responseDefault
            );
        }

        private Type? ResolveResponseType(Type? type)
        {
            if (type is null || type == typeof(Task))
                return null;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                return type.GetGenericArguments()[0];

            return type;
        }

        private (ResponseKind, Type?) ResolveResponseKindAndInnerType(Type? type)
        {
            if (type is null)
                return (ResponseKind.None, null);

            if (type.FullName == typeof(IResult).FullName)
                return (ResponseKind.Result, null);

            var isGenericResult = type.IsGenericType && type.GetGenericTypeDefinition().FullName == typeof(IResult<>).FullName;
            if (isGenericResult)
                return (ResponseKind.DataResult, type.GetGenericArguments()[0]);

            return (ResponseKind.Plain, type);
        }

        /// <summary>
        /// Returns type, assignable to given type, that can be used as default value (is struct or class with parameterless constructor)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tm"></param>
        /// <returns></returns>
        private Type? ResolveTypeDefaultType(Type? type, ITypeManager tm)
        {
            if (type is null)
                return null;

            if (type.IsValueType)
                return type;

            if (type.IsClass)
            {
                var classDefault = !type.IsAbstract && type.HasDefaultConstructor()
                    ? type
                    : tm.GetImplementations(type)
                        .FirstOrDefault(x => !x.IsAbstract && x.HasDefaultConstructor());

                return classDefault;
            }

            // special handling for IEnumerable<> and dictionaries
            var enumerableImplementation = type.GetTargetImplementation(typeof(IEnumerable<>));
            if (enumerableImplementation != null)
            {
                var elementType = enumerableImplementation.GetGenericArguments()[0];
                var keyValueImplementation = elementType.GetTargetImplementation(typeof(KeyValuePair<,>));

                var enumerableDefault = keyValueImplementation is null
                    ? elementType.MakeArrayType()
                    : typeof(Dictionary<,>).MakeGenericType(keyValueImplementation.GetGenericArguments());

                return enumerableDefault;
            }

            // special handling for IEnumerable and friends
            if (type.IsAssignableFrom(typeof(Array)))
                return typeof(Array);

            var defaultType = tm.GetImplementations(type)
                .FirstOrDefault(x => x.IsValueType || x.IsClass && !x.IsAbstract && x.HasDefaultConstructor());

            return defaultType;
        }

        private string GetDefaultExpression(ResponseKind kind, Type? responseType, Type? defaultType) => kind switch
        {
            ResponseKind.Plain  => GetDefaultExpression(defaultType),
            ResponseKind.Result => @"Result.New().Error(""Request failed"")",
            ResponseKind.DataResult => responseType!.FullName == defaultType!.FullName
                ? $@"Result.New({GetDefaultExpression(defaultType)}).Error(""Request failed"")"
                : $@"Result.New<{responseType.FriendlyName()}>({GetDefaultExpression(defaultType)}).Error(""Request failed"")",
            _ => string.Empty,
        };

        private string GetDefaultExpression(Type? type)
        {
            if (type is null)
                return string.Empty;

            if (type.IsValueType)
                return $"default({type.FriendlyName()})";

            if (type.IsArray)
                return $"Array.Empty<{type.GetElementType()!.FriendlyName()}>()";

            return $"new {type.FriendlyName()}()";
        }

        private enum ResponseKind
        {
            None,
            Plain,
            Result,
            DataResult,
        }
    }
}