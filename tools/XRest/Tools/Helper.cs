using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace xrest.Tools
{
    internal static class Helper
    {
        public static bool IsDictionary(Type type) => type.GetInterfaces().Any(x =>
        {
            if (!x.IsGenericType)
                return false;

            var definition = x.GetGenericTypeDefinition();

            return definition == typeof(Dictionary<,>) ||
                definition == typeof(IDictionary<,>) ||
                definition == typeof(ReadOnlyDictionary<,>) ||
                definition == typeof(IReadOnlyDictionary<,>);
        });

        public static bool IsArray(Type type) => type.GetInterfaces().Any(x =>
        {
            if (!x.IsGenericType)
                return false;

            var definition = x.GetGenericTypeDefinition();
            if (definition != typeof(IEnumerable<>))
                return false;

            return definition == typeof(IEnumerable<>) ||
                definition.IsArray ||
                definition == typeof(List<>) ||
                definition == typeof(LinkedList<>) ||
                definition == typeof(IList<>) ||
                definition == typeof(IReadOnlyList<>) ||
                definition == typeof(HashSet<>) ||
                definition == typeof(ISet<>) ||
                definition == typeof(Stack<>) ||
                definition == typeof(Collection<>) ||
                definition == typeof(ICollection<>) ||
                definition == typeof(ReadOnlyCollection<>) ||
                definition == typeof(IReadOnlyCollection<>);
        });

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
        public static bool IsIDisposableMethod(MethodInfo methodInfo)
        {
            // Ideally we do not want Dispose method to be exposed as an action. However there are some scenarios where a user
            // might want to expose a method with name "Dispose" (even though they might not be really disposing resources)
            // Example: A controller deriving from MVC's Controller type might wish to have a method with name Dispose,
            // in which case they can use the "new" keyword to hide the base controller's declaration.

            // Find where the method was originally declared
            var baseMethodInfo = methodInfo.GetBaseDefinition();
            var declaringTypeInfo = baseMethodInfo.DeclaringType.GetTypeInfo();

            return
                typeof(IDisposable).GetTypeInfo().IsAssignableFrom(declaringTypeInfo) &&
                declaringTypeInfo.GetRuntimeInterfaceMap(typeof(IDisposable)).TargetMethods[0] == baseMethodInfo;
        }
    }
}