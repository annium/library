using System;
using System.Collections.Generic;

namespace Annium.Components.State.Forms;

/// <summary>
/// Factory interface for creating different types of state containers.
/// </summary>
public interface IStateFactory
{
    /// <summary>
    /// Creates an atomic container for tracking simple values.
    /// </summary>
    /// <typeparam name="T">The type of value to track.</typeparam>
    /// <param name="defaultValue">The initial value for the container.</param>
    /// <returns>An atomic container initialized with the specified default value.</returns>
    IAtomicContainer<T> CreateAtomic<T>(T defaultValue);

    /// <summary>
    /// Creates an array container for tracking a list of objects.
    /// </summary>
    /// <typeparam name="T">The type of items in the list, which must be non-null and have a parameterless constructor.</typeparam>
    /// <param name="initialValue">The initial list value for the container.</param>
    /// <returns>An array container initialized with the specified initial value.</returns>
    IArrayContainer<T> CreateArray<T>(List<T> initialValue)
        where T : notnull, new();

    /// <summary>
    /// Creates a map container for tracking a dictionary of key-value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary keys, which must implement IEquatable.</typeparam>
    /// <typeparam name="TValue">The type of dictionary values, which must be non-null and have a parameterless constructor.</typeparam>
    /// <param name="initialValue">The initial dictionary value for the container.</param>
    /// <returns>A map container initialized with the specified initial value.</returns>
    IMapContainer<TKey, TValue> CreateMap<TKey, TValue>(Dictionary<TKey, TValue> initialValue)
        where TKey : IEquatable<TKey>
        where TValue : notnull, new();

    /// <summary>
    /// Creates an object container for tracking complex objects with nested properties.
    /// </summary>
    /// <typeparam name="T">The type of object to track, which must be non-null and have a parameterless constructor.</typeparam>
    /// <param name="initialValue">The initial object value for the container.</param>
    /// <returns>An object container initialized with the specified initial value.</returns>
    IObjectContainer<T> CreateObject<T>(T initialValue)
        where T : notnull, new();
}
