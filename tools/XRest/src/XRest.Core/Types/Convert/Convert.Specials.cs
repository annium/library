using System;
using System.Threading.Tasks;
using XRest.Core.Types.Models;

namespace XRest.Core.Helpers;

public static partial class TypeHelper
{
    private static ITypeModel? ResolveSpecialTypeModel(Type type)
    {
        return type.IsGenericType
            ? ResolveSpecialGenericTypeModel(type, type.GetGenericTypeDefinition())
            : ResolveSpecialNonGenericTypeModel(type);
    }

    private static ITypeModel? ResolveSpecialGenericTypeModel(Type type, Type definition)
    {
        if (definition == typeof(Task<>) || definition == typeof(ValueTask<>))
        {
            Console.WriteLine($"Resolve task type model for {type}");
            return ResolveTypeModel(type.GetGenericArguments()[0]);
        }

        if (IsArrayType(definition))
        {
            Console.WriteLine($"Resolve array type model for {type}");
            return ResolveArrayTypeModel(type);
        }

        if (IsRecordType(definition))
        {
            Console.WriteLine($"Resolve record type model for {type}");
            return ResolveRecordTypeModel(type);
        }

        return null;
    }

    private static ITypeModel? ResolveSpecialNonGenericTypeModel(Type type)
    {
        if (type == typeof(Task) || type == typeof(ValueTask))
            return BaseType.GetFor(typeof(void));

        return null;
    }
}