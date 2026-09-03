using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Components.State.Forms;

/// <summary>
/// Represents a container for tracking state changes of complex objects with nested properties.
/// </summary>
/// <typeparam name="T">The type of object being tracked, which must be non-null and have a parameterless constructor.</typeparam>
public interface IObjectContainer<T> : IValueTrackedState<T>
    where T : notnull, new()
{
    /// <summary>
    /// Gets a read-only dictionary of child tracked states by their property names.
    /// </summary>
    IReadOnlyDictionary<string, ITrackedState> Children { get; }

    /// <summary>
    /// Creates a nested object container for tracking a complex object property.
    /// </summary>
    /// <typeparam name="TI">The type of the nested object, which must be non-null and have a parameterless constructor.</typeparam>
    /// <param name="ex">An expression that selects the nested object property.</param>
    /// <returns>An object container for tracking the nested object.</returns>
    IObjectContainer<TI> AtObject<TI>(Expression<Func<T, TI>> ex)
        where TI : notnull, new();

    /// <summary>
    /// Creates an array container for tracking a list property.
    /// </summary>
    /// <typeparam name="TI">The type of items in the list, which must be non-null and have a parameterless constructor.</typeparam>
    /// <param name="ex">An expression that selects the list property.</param>
    /// <returns>An array container for tracking the list.</returns>
    IArrayContainer<TI> AtArray<TI>(Expression<Func<T, List<TI>>> ex)
        where TI : notnull, new();

    /// <summary>
    /// Creates a map container for tracking a dictionary property.
    /// </summary>
    /// <typeparam name="TK">The type of dictionary keys, which must be non-null.</typeparam>
    /// <typeparam name="TV">The type of dictionary values, which must be non-null and have a parameterless constructor.</typeparam>
    /// <param name="ex">An expression that selects the dictionary property.</param>
    /// <returns>A map container for tracking the dictionary.</returns>
    IMapContainer<TK, TV> AtMap<TK, TV>(Expression<Func<T, Dictionary<TK, TV>>> ex)
        where TK : notnull
        where TV : notnull, new();

    /// <summary>
    /// Creates an atomic container for tracking a simple property value.
    /// </summary>
    /// <typeparam name="TI">The type of the atomic property.</typeparam>
    /// <param name="ex">An expression that selects the atomic property.</param>
    /// <returns>An atomic container for tracking the property value.</returns>
    IAtomicContainer<TI> AtAtomic<TI>(Expression<Func<T, TI>> ex);
}
