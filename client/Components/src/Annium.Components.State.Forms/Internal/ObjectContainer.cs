using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Components.State.Core;
using Annium.Logging;

namespace Annium.Components.State.Forms.Internal;

/// <summary>
/// Represents a container for managing state of complex objects with multiple properties.
/// Automatically creates tracked state containers for each readable and writable property of type T.
/// </summary>
/// <typeparam name="T">The object type to be contained and tracked</typeparam>
internal class ObjectContainer<T> : ObservableState, IObjectContainer<T>, ILogSubject
    where T : notnull, new()
{
    /// <summary>
    /// Gets the cached array of readable and writable properties for type T.
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static PropertyInfo[] Properties { get; }

    /// <summary>
    /// Gets the cached dictionary mapping properties to their corresponding state factory methods.
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static IReadOnlyDictionary<PropertyInfo, MethodInfo> Factories { get; }

    /// <summary>
    /// Initializes static members by discovering properties and resolving their state factories.
    /// </summary>
    static ObjectContainer()
    {
        Properties = typeof(T).GetProperties().Where(x => x is { CanRead: true, CanWrite: true }).ToArray();
        Factories = Properties.ToDictionary(x => x, x => StateFactoryResolver.ResolveFactory(x.PropertyType));
    }

    /// <summary>
    /// Gets the current value by reconstructing the object from its tracked property states.
    /// </summary>
    public T Value => CreateValue();

    /// <summary>
    /// Gets a value indicating whether any property state has changed from its initial value.
    /// </summary>
    public bool HasChanged => _states.Values.Any(x => x.Ref.HasChanged);

    /// <summary>
    /// Gets a value indicating whether any property state has been touched (modified by user interaction).
    /// </summary>
    public bool HasBeenTouched => _states.Values.Any(x => x.Ref.HasBeenTouched);

    /// <summary>
    /// Gets a dictionary of child tracked states indexed by property name.
    /// </summary>
    public IReadOnlyDictionary<string, ITrackedState> Children =>
        _states.ToDictionary(x => x.Key.Name, x => x.Value.Ref);

    /// <summary>
    /// Gets the logger instance for this container.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The dictionary mapping properties to their state references containing tracked state and reflection methods.
    /// </summary>
    private readonly IReadOnlyDictionary<PropertyInfo, StateReference> _states;

    /// <summary>
    /// Initializes a new instance of the ObjectContainer class with the specified initial value.
    /// Creates tracked states for all readable and writable properties of the object.
    /// </summary>
    /// <param name="initialValue">The initial value to initialize the container with</param>
    /// <param name="stateFactory">The factory used to create tracked states for properties</param>
    /// <param name="logger">The logger instance for this container</param>
    public ObjectContainer(T initialValue, IStateFactory stateFactory, ILogger logger)
    {
        Logger = logger;
        var states = new Dictionary<PropertyInfo, StateReference>();
        _states = states;

        foreach (var property in Properties)
        {
            var create = Factories[property];
            var @ref = (ITrackedState)create.Invoke(stateFactory, [property.GetMethod!.Invoke(initialValue, [])!])!;
            @ref.Changed.Subscribe(_ => NotifyChanged());
            var type = @ref.GetType();
            var get = type.GetProperty(nameof(IValueTrackedState<object>.Value))!.GetMethod!;
            var set = type.GetMethod(nameof(IValueTrackedState<object>.Set))!;
            var init = type.GetMethod(nameof(IValueTrackedState<object>.Init))!;
            states[property] = new StateReference(@ref, get, set, init);
        }
    }

    /// <summary>
    /// Initializes all property states with values from the specified object without triggering change notifications.
    /// </summary>
    /// <param name="value">The object whose property values will be used for initialization</param>
    public void Init(T value)
    {
        using (Mute())
        {
            foreach (var property in Properties)
            {
                var state = _states[property];
                var propertyValue = property.GetMethod!.Invoke(value, [])!;
                state.Init.Invoke(state.Ref, [propertyValue]);
            }
        }

        NotifyChanged();
    }

    /// <summary>
    /// Sets all property states with values from the specified object and notifies of changes if any occurred.
    /// </summary>
    /// <param name="value">The object whose property values will be set</param>
    /// <returns>True if any property state changed; otherwise, false</returns>
    public bool Set(T value)
    {
        var changed = false;
        using (Mute())
        {
            foreach (var property in Properties)
            {
                var state = _states[property];
                var propertyValue = property.GetMethod!.Invoke(value, [])!;
                changed = (bool)state.Set.Invoke(state.Ref, [propertyValue])! || changed;
            }
        }

        if (changed)
            NotifyChanged();

        return changed;
    }

    /// <summary>
    /// Resets all property states to their initial values.
    /// </summary>
    public void Reset()
    {
        using (Mute())
        {
            foreach (var property in Properties)
                _states[property].Ref.Reset();
        }

        NotifyChanged();
    }

    /// <summary>
    /// Determines whether all property states have any of the specified statuses.
    /// </summary>
    /// <param name="statuses">The statuses to check against</param>
    /// <returns>True if all property states have any of the specified statuses; otherwise, false</returns>
    public bool IsStatus(params Status[] statuses)
    {
        foreach (var state in _states.Values)
            if (!state.Ref.IsStatus(statuses))
                return false;

        return true;
    }

    /// <summary>
    /// Determines whether any property state has any of the specified statuses.
    /// </summary>
    /// <param name="statuses">The statuses to check against</param>
    /// <returns>True if any property state has any of the specified statuses; otherwise, false</returns>
    public bool HasStatus(params Status[] statuses)
    {
        foreach (var state in _states.Values)
            if (state.Ref.HasStatus(statuses))
                return true;

        return false;
    }

    /// <summary>
    /// Gets the object container for a specific property identified by the expression.
    /// </summary>
    /// <typeparam name="TI">The type of the property</typeparam>
    /// <param name="ex">The expression identifying the property</param>
    /// <returns>The object container for the specified property</returns>
    public IObjectContainer<TI> AtObject<TI>(Expression<Func<T, TI>> ex)
        where TI : notnull, new()
    {
        return At<IObjectContainer<TI>>(ex);
    }

    /// <summary>
    /// Gets the array container for a specific list property identified by the expression.
    /// </summary>
    /// <typeparam name="TI">The element type of the list</typeparam>
    /// <param name="ex">The expression identifying the list property</param>
    /// <returns>The array container for the specified list property</returns>
    public IArrayContainer<TI> AtArray<TI>(Expression<Func<T, List<TI>>> ex)
        where TI : notnull, new()
    {
        return At<IArrayContainer<TI>>(ex);
    }

    /// <summary>
    /// Gets the map container for a specific dictionary property identified by the expression.
    /// </summary>
    /// <typeparam name="TK">The key type of the dictionary</typeparam>
    /// <typeparam name="TV">The value type of the dictionary</typeparam>
    /// <param name="ex">The expression identifying the dictionary property</param>
    /// <returns>The map container for the specified dictionary property</returns>
    public IMapContainer<TK, TV> AtMap<TK, TV>(Expression<Func<T, Dictionary<TK, TV>>> ex)
        where TK : notnull
        where TV : notnull, new()
    {
        return At<IMapContainer<TK, TV>>(ex);
    }

    /// <summary>
    /// Gets the atomic container for a specific property identified by the expression.
    /// </summary>
    /// <typeparam name="TI">The type of the property</typeparam>
    /// <param name="ex">The expression identifying the property</param>
    /// <returns>The atomic container for the specified property</returns>
    public IAtomicContainer<TI> AtAtomic<TI>(Expression<Func<T, TI>> ex)
    {
        return At<IAtomicContainer<TI>>(ex);
    }

    /// <summary>
    /// Creates a new instance of T and populates it with current values from all property states.
    /// </summary>
    /// <returns>A new instance of T with current property values</returns>
    private T CreateValue()
    {
        var value = new T();
        foreach (var property in Properties)
        {
            var state = _states[property];
            property.SetMethod!.Invoke(value, [state.Get.Invoke(state.Ref, [])]);
        }

        return value;
    }

    /// <summary>
    /// Gets the tracked state for a property identified by the lambda expression.
    /// </summary>
    /// <typeparam name="TX">The type of tracked state to return</typeparam>
    /// <param name="ex">The lambda expression identifying the property</param>
    /// <returns>The tracked state for the specified property</returns>
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

    /// <summary>
    /// Resolves a PropertyInfo from a lambda expression representing a direct property access.
    /// </summary>
    /// <param name="ex">The lambda expression to resolve</param>
    /// <returns>The PropertyInfo for the accessed property</returns>
    private PropertyInfo ResolveProperty(LambdaExpression ex)
    {
        if (ex.Body is MemberExpression { Member: PropertyInfo property })
            return property;

        throw new ArgumentException($"{ex} is not a direct property access expression");
    }

    /// <summary>
    /// Represents a reference to a tracked state along with reflection methods for accessing its value.
    /// </summary>
    private class StateReference
    {
        /// <summary>
        /// Gets the tracked state reference.
        /// </summary>
        public ITrackedState Ref { get; }

        /// <summary>
        /// Gets the reflection method for getting the tracked state's value.
        /// </summary>
        public MethodInfo Get { get; }

        /// <summary>
        /// Gets the reflection method for setting the tracked state's value.
        /// </summary>
        public MethodInfo Set { get; }

        /// <summary>
        /// Gets the reflection method for initializing the tracked state's value.
        /// </summary>
        public MethodInfo Init { get; }

        /// <summary>
        /// Initializes a new instance of the StateReference class.
        /// </summary>
        /// <param name="ref">The tracked state reference</param>
        /// <param name="get">The method for getting the tracked state's value</param>
        /// <param name="set">The method for setting the tracked state's value</param>
        /// <param name="init">The method for initializing the tracked state's value</param>
        public StateReference(ITrackedState @ref, MethodInfo get, MethodInfo set, MethodInfo init)
        {
            Ref = @ref;
            Get = get;
            Set = set;
            Init = init;
        }

        /// <summary>
        /// Returns a string representation of the tracked state type.
        /// </summary>
        /// <returns>The friendly name of the tracked state type</returns>
        public override string ToString() => Ref.GetType().FriendlyName();
    }
}
