using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Components.State.Core;
using Annium.Logging;

namespace Annium.Components.State.Forms.Internal;

internal class ObjectContainer<T> : ObservableState, IObjectContainer<T>, ILogSubject
    where T : notnull, new()
{
    // ReSharper disable once StaticMemberInGenericType
    private static PropertyInfo[] Properties { get; }

    // ReSharper disable once StaticMemberInGenericType
    private static IReadOnlyDictionary<PropertyInfo, MethodInfo> Factories { get; }

    static ObjectContainer()
    {
        Properties = typeof(T).GetProperties().Where(x => x is { CanRead: true, CanWrite: true }).ToArray();
        Factories = Properties.ToDictionary(x => x, x => StateFactoryResolver.ResolveFactory(x.PropertyType));
    }

    public T Value => CreateValue();
    public bool HasChanged => _states.Values.Any(x => x.Ref.HasChanged);
    public bool HasBeenTouched => _states.Values.Any(x => x.Ref.HasBeenTouched);
    public IReadOnlyDictionary<string, ITrackedState> Children => _states.ToDictionary(x => x.Key.Name, x => x.Value.Ref);
    public ILogger Logger { get; }
    private readonly IReadOnlyDictionary<PropertyInfo, StateReference> _states;

    public ObjectContainer(
        T initialValue,
        IStateFactory stateFactory,
        ILogger logger
    )
    {
        Logger = logger;
        var states = new Dictionary<PropertyInfo, StateReference>();
        _states = states;

        foreach (var property in Properties)
        {
            var create = Factories[property];
            var @ref = (ITrackedState)create.Invoke(stateFactory, new[] { property.GetMethod!.Invoke(initialValue, Array.Empty<object>())! })!;
            @ref.Changed.Subscribe(_ => NotifyChanged());
            var type = @ref.GetType();
            var get = type.GetProperty(nameof(IValueTrackedState<object>.Value))!.GetMethod!;
            var set = type.GetMethod(nameof(IValueTrackedState<object>.Set))!;
            var init = type.GetMethod(nameof(IValueTrackedState<object>.Init))!;
            states[property] = new StateReference(@ref, get, set, init);
        }
    }

    public void Init(T value)
    {
        using (Mute())
        {
            foreach (var property in Properties)
            {
                var state = _states[property];
                var propertyValue = property.GetMethod!.Invoke(value, Array.Empty<object>())!;
                state.Init.Invoke(state.Ref, new[] { propertyValue });
            }
        }

        NotifyChanged();
    }

    public bool Set(T value)
    {
        var changed = false;
        using (Mute())
        {
            foreach (var property in Properties)
            {
                var state = _states[property];
                var propertyValue = property.GetMethod!.Invoke(value, Array.Empty<object>())!;
                changed = (bool)state.Set.Invoke(state.Ref, new[] { propertyValue })! || changed;
            }
        }

        if (changed)
            NotifyChanged();

        return changed;
    }

    public void Reset()
    {
        using (Mute())
        {
            foreach (var property in Properties)
                _states[property].Ref.Reset();
        }

        NotifyChanged();
    }

    public bool IsStatus(params Status[] statuses)
    {
        foreach (var state in _states.Values)
            if (!state.Ref.IsStatus(statuses))
                return false;

        return true;
    }

    public bool HasStatus(params Status[] statuses)
    {
        foreach (var state in _states.Values)
            if (state.Ref.HasStatus(statuses))
                return true;

        return false;
    }

    public IObjectContainer<TI> AtObject<TI>(Expression<Func<T, TI>> ex)
        where TI : notnull, new()
    {
        return At<IObjectContainer<TI>>(ex);
    }

    public IArrayContainer<TI> AtArray<TI>(Expression<Func<T, List<TI>>> ex)
        where TI : notnull, new()
    {
        return At<IArrayContainer<TI>>(ex);
    }

    public IMapContainer<TK, TV> AtMap<TK, TV>(Expression<Func<T, Dictionary<TK, TV>>> ex)
        where TK : notnull
        where TV : notnull, new()
    {
        return At<IMapContainer<TK, TV>>(ex);
    }

    public IAtomicContainer<TI> AtAtomic<TI>(Expression<Func<T, TI>> ex)
    {
        return At<IAtomicContainer<TI>>(ex);
    }

    private T CreateValue()
    {
        var value = new T();
        foreach (var property in Properties)
        {
            var state = _states[property];
            property.SetMethod!.Invoke(value, new[] { state.Get.Invoke(state.Ref, Array.Empty<object>()) });
        }

        return value;
    }

    private TX At<TX>(LambdaExpression ex)
        where TX : ITrackedState
    {
        try
        {
            return (TX)_states[ResolveProperty(ex)].Ref;
        }
        catch (Exception e)
        {
            this.Error(e);
            throw;
        }
    }

    private PropertyInfo ResolveProperty(LambdaExpression ex)
    {
        if (ex.Body is MemberExpression { Member: PropertyInfo property })
            return property;

        throw new ArgumentException($"{ex} is not a direct property access expression");
    }

    private class StateReference
    {
        public ITrackedState Ref { get; }
        public MethodInfo Get { get; }
        public MethodInfo Set { get; }
        public MethodInfo Init { get; }

        public StateReference(
            ITrackedState @ref,
            MethodInfo get,
            MethodInfo set,
            MethodInfo init
        )
        {
            Ref = @ref;
            Get = get;
            Set = set;
            Init = init;
        }

        public override string ToString() => Ref.GetType().FriendlyName();
    }
}