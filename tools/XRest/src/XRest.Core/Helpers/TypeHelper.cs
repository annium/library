using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Primitives;
using XRest.Core.Extensions;
using XRest.Core.Models.Types;

namespace XRest.Core.Helpers;

public static partial class TypeHelper
{
    private static readonly ConcurrentDictionary<Type, ITypeModel> TypeModels = new();

    static TypeHelper()
    {
        RegisterBasicIgnoredTypes();
        RegisterBasicArrayTypes();
        RegisterBasicRecordTypes();
    }

    public static ITypeModel GetTypeModel(Type type) => TypeModels.GetOrAdd(type, ResolveTypeModel);

    private static ITypeModel ResolveTypeModel(Type type)
    {
        Console.WriteLine($"Resolve model for {type}");
        var baseType = BaseType.GetFor(type);
        if (baseType is not null)
        {
            Console.WriteLine($"Resolved base type model for {type}");
            return baseType;
        }

        if (type.IsGenericParameter)
        {
            Console.WriteLine($"Resolved generic parameter model for {type}");
            return new GenericParameterModel(type.Name);
        }

        if (type.IsEnum)
        {
            var names = Enum.GetNames(type);
            var rawValues = Enum.GetValuesAsUnderlyingType(type);

            var values = new Dictionary<string, long>();
            var i = 0;
            foreach (var value in rawValues)
                values[names[i++]] = Convert.ToInt64(value);

            Console.WriteLine($"Resolved enum model for {type}");
            return new EnumModel(type.GetNamespace(), type.FriendlyName(), values);
        }

        var specialType = ResolveSpecialTypeModel(type);
        if (specialType is not null)
            return specialType;

        // struct

        var name = type.FriendlyName();
        if (type.IsGenericType)
            name = name[..name.IndexOf('<')];
        var builder = StructModelBuilder.Init(type.GetNamespace(), name);

        var genericArguments = type.GetGenericArguments().Select(GetTypeModel).ToArray();
        builder.GenericArguments(genericArguments);

        if (type.BaseType is not null && !IsIgnoredType(type.BaseType))
            builder.Base((StructModel) GetTypeModel(type.BaseType));

        var interfaces = type.GetInterfaces()
            .Where(x => !IsIgnoredType(x))
            .Select(GetTypeModel)
            .OfType<StructModel>()
            .ToArray();
        builder.Interfaces(interfaces);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => new FieldModel(GetTypeModel(x.PropertyType), x.Name))
            .ToArray();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => new FieldModel(GetTypeModel(x.FieldType), x.Name))
            .ToArray();
        builder.Fields(properties.Concat(fields).ToArray());

        var model = builder.Build();

        return model;
    }
}