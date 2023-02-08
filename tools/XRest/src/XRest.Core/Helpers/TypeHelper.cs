using System;
using Annium.Core.Primitives;
using XRest.Core.Models.Types;

namespace XRest.Core.Helpers;

public static class TypeHelper
{
    public static ITypeModel GetTypeModel<T>() => GetTypeModel(typeof(T));

    public static ITypeModel GetTypeModel(Type type)
    {
        var baseType = BaseType.GetFor(type);
        if (baseType is not null)
            return baseType;

        throw new ArgumentException($"Can't resolve type model for {type.FriendlyName()}");
    }
}