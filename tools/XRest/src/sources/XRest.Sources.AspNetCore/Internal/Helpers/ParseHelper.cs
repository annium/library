using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace XRest.Sources.AspNetCore.Internal.Helpers;

public static class ParseHelper
{
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
        if (Shared.Helpers.ParseHelper.IsIDisposableMethod(methodInfo))
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
}
