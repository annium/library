using System;
using System.Threading;
using Annium.Net.Types;

namespace Annium.XRest.Sources.Shared.Helpers;

public static class ParseHelper
{
    public static bool IsAllowedQueryType(Type type, IMapperConfig config)
    {
        if (IsAllowed(type))
            return true;

        if (type.TryGetArrayElementType(out var elementType))
            return IsAllowed(elementType);

        return false;

        bool IsAllowed(Type x)
        {
            return config.IsBaseType(x) || x.IsEnum;
        }
    }

    public static bool IsSkippedType(Type type)
    {
        return type == typeof(CancellationToken);
    }
}
