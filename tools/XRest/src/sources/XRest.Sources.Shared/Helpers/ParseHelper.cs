using System;
using System.Reflection;
using System.Threading;
using Annium.Core.Primitives;
using Annium.Net.Types;

namespace XRest.Sources.Shared.Helpers;

public static class ParseHelper
{
    public static bool IsAllowedQueryType(Type type)
    {
        if (IsAllowed(type))
            return true;

        if (type.TryGetArrayElementType(out var elementType))
            return IsAllowed(elementType);

        return false;

        static bool IsAllowed(Type type)
        {
            return MapperConfig.IsBaseType(type) || type.IsEnum;
        }
    }

    public static bool IsSkippedType(Type type)
    {
        return type == typeof(CancellationToken);
    }

    public static bool IsIDisposableMethod(MethodInfo methodInfo)
    {
        // Ideally we do not want Dispose method to be exposed as an action. However there are some scenarios where a user
        // might want to expose a method with name "Dispose" (even though they might not be really disposing resources)
        // Example: A controller might wish to have a method with name Dispose,
        // in which case they can use the "new" keyword to hide the base controller's declaration.

        // Find where the method was originally declared
        var baseMethodInfo = methodInfo.GetBaseDefinition();
        var declaringTypeInfo = baseMethodInfo.DeclaringType!.GetTypeInfo();

        return
            typeof(IDisposable).GetTypeInfo().IsAssignableFrom(declaringTypeInfo) &&
            declaringTypeInfo.GetRuntimeInterfaceMap(typeof(IDisposable)).TargetMethods[0] == baseMethodInfo;
    }
}