using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Components.State.Core;

public static class StateObserver
{
    private static readonly object[] _emptyArgs = [];

    private static readonly ConcurrentDictionary<
        Type,
        IReadOnlyCollection<Func<object, IObservableState>>
    > _observableAccessors = new();

    public static IDisposable ObserveObject<T>(T target, Action handleChange)
        where T : class
    {
        var accessors = _observableAccessors.GetOrAdd(target.GetType(), DiscoverObservableAccessors);

        var disposable = Disposable.Box(VoidLogger.Instance);
        foreach (var get in accessors)
            disposable += get(target).Changed.Subscribe(_ => handleChange());

        return disposable;
    }

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
