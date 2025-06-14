using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Components.State.Core;

/// <summary>
/// Provides functionality to observe objects containing IObservableState properties and fields.
/// </summary>
public static class StateObserver
{
    /// <summary>
    /// Empty arguments array used for reflection method invocation.
    /// </summary>
    private static readonly object[] _emptyArgs = [];

    /// <summary>
    /// Cache of observable accessor functions keyed by object type.
    /// </summary>
    private static readonly ConcurrentDictionary<
        Type,
        IReadOnlyCollection<Func<object, IObservableState>>
    > _observableAccessors = new();

    /// <summary>
    /// Observes an object for changes in its IObservableState properties and fields.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <param name="target">The object to observe.</param>
    /// <param name="handleChange">The action to invoke when any observable state changes.</param>
    /// <returns>A disposable to stop observing.</returns>
    public static IDisposable ObserveObject<T>(T target, Action handleChange)
        where T : class
    {
        var accessors = _observableAccessors.GetOrAdd(target.GetType(), DiscoverObservableAccessors);

        var disposable = Disposable.Box(VoidLogger.Instance);
        foreach (var get in accessors)
            disposable += get(target).Changed.Subscribe(_ => handleChange());

        return disposable;
    }

    /// <summary>
    /// Discovers all IObservableState properties and fields in the specified type.
    /// </summary>
    /// <param name="type">The type to analyze.</param>
    /// <returns>A collection of accessor functions for observable states.</returns>
    private static IReadOnlyCollection<Func<object, IObservableState>> DiscoverObservableAccessors(Type type)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var accessors = new List<Func<object, IObservableState>>();

        var properties = type.GetProperties(flags)
            .Where(x => x.PropertyType.IsDerivedFrom(typeof(IObservableState)))
            .ToArray();
        foreach (var property in properties)
            accessors.Add(instance => (IObservableState)property.GetMethod!.Invoke(instance, _emptyArgs)!);

        var fields = type.GetFields(flags).Where(x => x.FieldType.IsDerivedFrom(typeof(IObservableState))).ToArray();
        foreach (var field in fields)
            accessors.Add(instance => (IObservableState)field.GetValue(instance)!);

        return accessors;
    }
}
