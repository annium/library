using System;
using System.Linq;
using System.Reflection;
using Annium.Reflection;

namespace Annium.Components.State.Forms.Internal;

/// <summary>
/// Provides functionality to resolve the appropriate state factory method for a given type.
/// Uses reflection to determine the correct container type (atomic, array, map, or object) based on type characteristics.
/// </summary>
internal static class StateFactoryResolver
{
    /// <summary>
    /// The cached MethodInfo for the atomic factory method.
    /// </summary>
    private static readonly MethodInfo _atomicFactory = GetFactory(nameof(IStateFactory.CreateAtomic));

    /// <summary>
    /// The cached MethodInfo for the array factory method.
    /// </summary>
    private static readonly MethodInfo _arrayFactory = GetFactory(nameof(IStateFactory.CreateArray));

    /// <summary>
    /// The cached MethodInfo for the map factory method.
    /// </summary>
    private static readonly MethodInfo _mapFactory = GetFactory(nameof(IStateFactory.CreateMap));

    /// <summary>
    /// The cached MethodInfo for the object factory method.
    /// </summary>
    private static readonly MethodInfo _objectFactory = GetFactory(nameof(IStateFactory.CreateObject));

    /// <summary>
    /// Resolves the appropriate state factory method for the specified type.
    /// Determines the correct container type based on type characteristics and returns the corresponding factory method.
    /// </summary>
    /// <param name="type">The type to resolve a factory method for</param>
    /// <returns>The MethodInfo for the appropriate state factory method</returns>
    public static MethodInfo ResolveFactory(Type type)
    {
        if (type.IsRecordLike())
            return ResolveFactory(_mapFactory, type);

        if (type.IsArrayLike())
            return ResolveFactory(_arrayFactory, type);

        if (type.GetInterfaces().Contains(typeof(IEquatable<>).MakeGenericType(type)))
            return ResolveFactory(_atomicFactory, type);

        if (type.IsEnum)
            return ResolveFactory(_atomicFactory, type);

        return ResolveFactory(_objectFactory, type);
    }

    /// <summary>
    /// Resolves a specific factory method by making it generic with the appropriate type arguments.
    /// Handles both simple generic methods and complex generic parameter resolution.
    /// </summary>
    /// <param name="factory">The factory method to resolve</param>
    /// <param name="type">The type to use for generic argument resolution</param>
    /// <returns>The resolved MethodInfo with proper generic arguments</returns>
    private static MethodInfo ResolveFactory(MethodInfo factory, Type type)
    {
        var template = factory.GetParameters().Single().ParameterType;
        if (template.IsGenericParameter)
            return factory.MakeGenericMethod(type);

        var args = template.ResolveGenericArgumentsByImplementation(type);

        if (args is null)
            throw new InvalidOperationException($"Failed to resolve state factory for {type}");

        var parameters = factory.GetGenericArguments();
        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var arg = args[i];
            if (param.ResolveByImplementation(arg) is null)
                throw new InvalidOperationException($"Failed to resolve state factory for {type}");
        }

        return factory.MakeGenericMethod(args);
    }

    /// <summary>
    /// Gets the factory method from IStateFactory interface by name.
    /// </summary>
    /// <param name="name">The name of the factory method to retrieve</param>
    /// <returns>The MethodInfo for the specified factory method</returns>
    private static MethodInfo GetFactory(string name) => typeof(IStateFactory).GetMethods().Single(x => x.Name == name);
}
