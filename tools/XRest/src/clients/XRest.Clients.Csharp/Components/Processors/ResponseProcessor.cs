using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Data.Operations;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class ResponseProcessor
{
    private const string KindNone = "none";
    private const string KindPlain = "plain";

    public static (string responseType, string responseDefault) Resolve(IRef response, ClientContext ctx)
    {
        var responseRef = response is PromiseRef { Value: { } } promiseResponse ? promiseResponse.Value : null;

        var type = string.Empty;
        if (responseRef is not null)
            type = RefProcessor.Process(responseRef, ctx);

        return (type, string.Empty);
    }

    private static (string, IRef?) ResolveResponseKindAndInnerType(IRef? response, ClientContext ctx)
    {
        if (response is null)
            return (KindNone, null);

        var special = ResponseSpecialProcessor.ResolveResponseKindAndInnerType(response, ctx);

        return special ?? (KindPlain, response);
    }

    /// <summary>
    /// Returns type, assignable to given type, that can be used as default value (is struct or class with parameterless constructor)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="tm"></param>
    /// <param name="addUsage"></param>
    /// <returns></returns>
    private static Type? ResolveTypeDefaultType(Type? type, ITypeManager tm, Action<Namespace> addUsage)
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

    private static string GetDefaultExpression(ResponseKind kind, Type? responseType, Type? defaultType, Action<Namespace> addUsage) => kind switch
    {
        ResponseKind.Plain  => GetDefaultExpression(defaultType, addUsage),
        ResponseKind.Result => @"Result.New().Error(""Request failed"")",
        ResponseKind.DataResult => responseType!.FullName == defaultType!.FullName
            ? $@"Result.New({GetDefaultExpression(defaultType, addUsage)}).Error(""Request failed"")"
            : $@"Result.New<{responseType.FriendlyName()}>({GetDefaultExpression(defaultType, addUsage)}).Error(""Request failed"")",
        _ => string.Empty,
    };

    private static string GetDefaultExpression(Type? type, Action<Namespace> addUsage)
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