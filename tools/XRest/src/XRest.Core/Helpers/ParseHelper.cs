using System;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace XRest.Core.Helpers;

public static class ParseHelper
{
    public static bool IsAllowedQueryType(Type type)
    {
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(Instant) ||
               type == typeof(Guid);
    }

    public static bool IsSkippedType(Type type)
    {
        return type == typeof(CancellationToken);
    }

    /// <summary>
    ///    From https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Core/src/ApplicationModels/DefaultApplicationModelProvider.cs
    /// </summary>
    public static bool IsAction(MethodInfo methodInfo)
    {
        // The SpecialName bit is set to flag members that are treated in a special way by some compilers
        // (such as property accessors and operator overloading methods).
        if (methodInfo.IsSpecialName)
        {
            return false;
        }

        if (methodInfo.IsDefined(typeof(NonActionAttribute)))
        {
            return false;
        }

        // Overridden methods from Object class, e.g. Equals(Object), GetHashCode(), etc., are not valid.
        if (methodInfo.GetBaseDefinition().DeclaringType == typeof(object))
        {
            return false;
        }

        // Dispose method implemented from IDisposable is not valid
        if (IsIDisposableMethod(methodInfo))
        {
            return false;
        }

        if (methodInfo.IsAbstract)
        {
            return false;
        }

        if (methodInfo.IsConstructor)
        {
            return false;
        }

        if (methodInfo.IsGenericMethod)
        {
            return false;
        }

        return methodInfo.IsPublic;
    }

    /// <summary>
    ///    From https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Core/src/ApplicationModels/DefaultApplicationModelProvider.cs
    /// </summary>
    private static bool IsIDisposableMethod(MethodInfo methodInfo)
    {
        // Ideally we do not want Dispose method to be exposed as an action. However there are some scenarios where a user
        // might want to expose a method with name "Dispose" (even though they might not be really disposing resources)
        // Example: A controller deriving from MVC's Controller type might wish to have a method with name Dispose,
        // in which case they can use the "new" keyword to hide the base controller's declaration.

        // Find where the method was originally declared
        var baseMethodInfo = methodInfo.GetBaseDefinition();
        var declaringTypeInfo = baseMethodInfo.DeclaringType!.GetTypeInfo();

        return
            typeof(IDisposable).GetTypeInfo().IsAssignableFrom(declaringTypeInfo) &&
            declaringTypeInfo.GetRuntimeInterfaceMap(typeof(IDisposable)).TargetMethods[0] == baseMethodInfo;
    }
}