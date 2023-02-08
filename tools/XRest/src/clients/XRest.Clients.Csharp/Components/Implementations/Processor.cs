using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Data.Operations;
using XRest.Clients.Csharp.Extensions;
using XRest.Clients.Csharp.Views;
using XRest.Core.Extensions;
using XRest.Core.Models;
using static XRest.Clients.Csharp.Helpers.ProcessorHelper;

namespace XRest.Clients.Csharp.Components.Implementations;

internal class Processor : IProcessor
{
    private static readonly Namespace AnniumNetHttp = "Annium.Net.Http".ToNamespace();
    private const string Client = "Client";
    private const string Root = "Root";

    public ClientContainerView Process(Namespace ns, ApiModel api)
    {
        var tm = TypeManager.GetInstance(api.Assembly);
        var candidates = api.Controllers
            .Select(x => ParseController(ns, x, tm))
            .ToArray();

        var node = BuildClientNode(ns, Client, Root, candidates);

        return node;
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

    private ClientCandidate ParseController(Namespace rootNs, ControllerModel controller, ITypeManager tm)
    {
        var ns = rootNs.Concat(controller.Namespace).ToNamespace();
        var namespaces = new List<Namespace>();
        var actions = controller.Actions.Select(x => ParseAction(x, tm, namespaces.Add)).ToArray();
        namespaces.AddRange(controller.Actions
            .SelectMany(x => CollectReferences(x, tm, namespaces.Add))
            .Distinct()
            .Where(x => !IsBaseType(x))
            .Select(x => x.Namespace!)
            .Append("Annium.Net.Http")
            .Append("System.Threading.Tasks")
            .Select(Namespace.New));
        var usages = namespaces.ToUsagesFrom(ns);

        return new ClientCandidate(
            usages,
            ns,
            controller.Name,
            $"{controller.Name}{Client}",
            actions
        );
    }

    private IReadOnlyCollection<Type> CollectReferences(ActionModel action, ITypeManager tm, Action<Namespace> addUsage)
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
            var (_, responseType) = ResolveResponseKindAndInnerType(response, addUsage);
            var defaultType = ResolveTypeDefaultType(responseType, tm, addUsage);
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


    private ActionView ParseAction(ActionModel action, ITypeManager tm, Action<Namespace> addUsage)
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
        var (kind, responseType) = ResolveResponseKindAndInnerType(response, addUsage);
        var defaultType = ResolveTypeDefaultType(responseType, tm, addUsage);
        var responseDefault = GetDefaultExpression(kind, responseType, defaultType, addUsage);

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

    private (ResponseKind, Type?) ResolveResponseKindAndInnerType(Type? type, Action<Namespace> addUsage)
    {
        if (type is null)
            return (ResponseKind.None, null);

        if (type.FullName == typeof(IResult).FullName)
        {
            addUsage(typeof(IResult).GetNamespace());

            return (ResponseKind.Result, null);
        }

        var isGenericResult = type.IsGenericType && type.GetGenericTypeDefinition().FullName == typeof(IResult<>).FullName;
        if (isGenericResult)
        {
            addUsage(typeof(IResult<>).GetNamespace());

            return (ResponseKind.DataResult, type.GetGenericArguments()[0]);
        }

        return (ResponseKind.Plain, type);
    }

    /// <summary>
    /// Returns type, assignable to given type, that can be used as default value (is struct or class with parameterless constructor)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="tm"></param>
    /// <param name="addUsage"></param>
    /// <returns></returns>
    private Type? ResolveTypeDefaultType(Type? type, ITypeManager tm, Action<Namespace> addUsage)
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

            if (keyValueImplementation is null)
            {
                addUsage(typeof(Array).GetNamespace());

                return elementType.MakeArrayType();
            }

            addUsage(typeof(Dictionary<,>).GetNamespace());

            return typeof(Dictionary<,>).MakeGenericType(keyValueImplementation.GetGenericArguments());
        }

        // special handling for IEnumerable and friends
        if (type.IsAssignableFrom(typeof(Array)))
            return typeof(Array);

        var defaultType = tm.GetImplementations(type)
            .FirstOrDefault(x => x.IsValueType || x.IsClass && !x.IsAbstract && x.HasDefaultConstructor());

        return defaultType;
    }

    private string GetDefaultExpression(ResponseKind kind, Type? responseType, Type? defaultType, Action<Namespace> addUsage) => kind switch
    {
        ResponseKind.Plain  => GetDefaultExpression(defaultType, addUsage),
        ResponseKind.Result => @"Result.New().Error(""Request failed"")",
        ResponseKind.DataResult => responseType!.FullName == defaultType!.FullName
            ? $@"Result.New({GetDefaultExpression(defaultType, addUsage)}).Error(""Request failed"")"
            : $@"Result.New<{responseType.FriendlyName()}>({GetDefaultExpression(defaultType, addUsage)}).Error(""Request failed"")",
        _ => string.Empty,
    };

    private string GetDefaultExpression(Type? type, Action<Namespace> addUsage)
    {
        if (type is null)
            return string.Empty;

        addUsage(type.GetNamespace());

        if (type.IsValueType)
            return $"default({type.FriendlyName()})";

        if (type.IsArray)
        {
            addUsage(typeof(Array).GetNamespace());

            return $"Array.Empty<{type.GetElementType()!.FriendlyName()}>()";
        }

        return $"({type.FriendlyName()}) Activator.CreateInstance(typeof({type.FriendlyName()}), true)!";
    }

    private enum ResponseKind
    {
        None,
        Plain,
        Result,
        DataResult,
    }
}