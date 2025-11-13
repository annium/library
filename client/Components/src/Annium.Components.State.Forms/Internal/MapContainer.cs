using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Components.State.Core;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions;
using Annium.Logging;

namespace Annium.Components.State.Forms.Internal;

/// <summary>
/// Represents a state container for managing dictionaries with change tracking, validation, and nested property access.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary, must be non-null.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary, must be non-null and have a parameterless constructor.</typeparam>
internal class MapContainer<TKey, TValue> : ObservableState, IMapContainer<TKey, TValue>, ILogSubject
    where TKey : notnull
    where TValue : notnull, new()
{
    /// <summary>
    /// Gets the factory method for creating state instances of type TValue.
    /// </summary>
    private static MethodInfo Factory { get; } = StateFactoryResolver.ResolveFactory(typeof(TValue));

    /// <summary>
    /// Gets the current value of the dictionary.
    /// </summary>
    public Dictionary<TKey, TValue> Value => CreateValue();

    /// <summary>
    /// Gets a value indicating whether the dictionary has changed from its initial value.
    /// </summary>
    public bool HasChanged => !Value.IsShallowEqual(_initialValue, _mapper);

    /// <summary>
    /// Gets a value indicating whether the dictionary or any of its values have been touched (modified).
    /// </summary>
    public bool HasBeenTouched
    {
        get => field || _states.Values.Any(x => x.Ref.HasBeenTouched);
        private set;
    }

    /// <summary>
    /// Gets the keys of the dictionary.
    /// </summary>
    public IReadOnlyCollection<TKey> Keys => _states.Keys.ToArray();

    /// <summary>
    /// Gets the logger instance for this container.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The state factory used to create new state instances.
    /// </summary>
    private readonly IStateFactory _stateFactory;

    /// <summary>
    /// The mapper instance used for value comparison and transformation.
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// The collection of state references for individual dictionary values.
    /// </summary>
    private readonly IDictionary<TKey, StateReference> _states;

    /// <summary>
    /// The initial value of the dictionary.
    /// </summary>
    private Dictionary<TKey, TValue> _initialValue;

    /// <summary>
    /// Initializes a new instance of the MapContainer class.
    /// </summary>
    /// <param name="initialValue">The initial value of the dictionary.</param>
    /// <param name="stateFactory">The state factory for creating new state instances.</param>
    /// <param name="mapper">The mapper for value comparison and transformation.</param>
    /// <param name="logger">The logger instance.</param>
    public MapContainer(
        Dictionary<TKey, TValue> initialValue,
        IStateFactory stateFactory,
        IMapper mapper,
        ILogger logger
    )
    {
        _initialValue = initialValue;
        _stateFactory = stateFactory;
        _mapper = mapper;
        Logger = logger;
        _states = new Dictionary<TKey, StateReference>();
        Init(_initialValue);
    }

    /// <summary>
    /// Initializes the dictionary container with a new value, resetting the touched state.
    /// </summary>
    /// <param name="value">The new value to initialize with.</param>
    public void Init(Dictionary<TKey, TValue> value)
    {
        _initialValue = value;
        using (Mute())
        {
            foreach (var key in _states.Keys.ToArray())
                if (!value.ContainsKey(key))
                    _states.Remove(key);
            foreach (var (key, item) in value)
            {
                if (_states.TryGetValue(key, out var state))
                    state.Ref.Init(item);
                else
                    AddInternal(key, item);
            }
        }

        HasBeenTouched = false;

        NotifyChanged();
    }

    /// <summary>
    /// Sets a new value for the dictionary, marking it as touched if changes occur.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    /// <returns>True if the value was changed, false otherwise.</returns>
    public bool Set(Dictionary<TKey, TValue> value)
    {
        var changed = false;
        using (Mute())
        {
            foreach (var key in _states.Keys.ToArray())
                if (!value.ContainsKey(key))
                    _states.Remove(key);
            foreach (var (key, item) in value)
            {
                if (_states.TryGetValue(key, out var state))
                    changed = state.Ref.Set(item) || changed;
                else
                {
                    AddInternal(key, item);
                    changed = true;
                }
            }
        }

        if (changed)
        {
            HasBeenTouched = true;
            NotifyChanged();
        }

        return changed;
    }

    /// <summary>
    /// Resets the dictionary to its initial value.
    /// </summary>
    public void Reset() => Init(_initialValue);

    /// <summary>
    /// Checks if all values in the dictionary have any of the specified statuses.
    /// </summary>
    /// <param name="statuses">The statuses to check for.</param>
    /// <returns>True if all values have any of the specified statuses, false otherwise.</returns>
    public bool IsStatus(params Status[] statuses)
    {
        foreach (var state in _states.Values)
            if (!state.Ref.IsStatus(statuses))
                return false;

        return true;
    }

    /// <summary>
    /// Checks if any value in the dictionary has any of the specified statuses.
    /// </summary>
    /// <param name="statuses">The statuses to check for.</param>
    /// <returns>True if any value has any of the specified statuses, false otherwise.</returns>
    public bool HasStatus(params Status[] statuses)
    {
        foreach (var state in _states.Values)
            if (state.Ref.HasStatus(statuses))
                return true;

        return false;
    }

    /// <summary>
    /// Gets an object container for a nested object property accessed via expression.
    /// </summary>
    /// <typeparam name="TI">The type of the nested object.</typeparam>
    /// <param name="ex">The expression to access the nested object.</param>
    /// <returns>An object container for the specified nested object.</returns>
    public IObjectContainer<TI> AtObject<TI>(Expression<Func<Dictionary<TKey, TValue>, TI>> ex)
        where TI : notnull, new()
    {
        return At<IObjectContainer<TI>>(ex);
    }

    /// <summary>
    /// Gets an array container for a nested array property accessed via expression.
    /// </summary>
    /// <typeparam name="TI">The type of items in the nested array.</typeparam>
    /// <param name="ex">The expression to access the nested array.</param>
    /// <returns>An array container for the specified nested array.</returns>
    public IArrayContainer<TI> AtArray<TI>(Expression<Func<Dictionary<TKey, TValue>, List<TI>>> ex)
        where TI : notnull, new()
    {
        return At<IArrayContainer<TI>>(ex);
    }

    /// <summary>
    /// Gets a map container for a nested dictionary property accessed via expression.
    /// </summary>
    /// <typeparam name="TK">The type of keys in the nested dictionary.</typeparam>
    /// <typeparam name="TV">The type of values in the nested dictionary.</typeparam>
    /// <param name="ex">The expression to access the nested dictionary.</param>
    /// <returns>A map container for the specified nested dictionary.</returns>
    public IMapContainer<TK, TV> AtMap<TK, TV>(Expression<Func<Dictionary<TKey, TValue>, Dictionary<TK, TV>>> ex)
        where TK : notnull
        where TV : notnull, new()
    {
        return At<IMapContainer<TK, TV>>(ex);
    }

    /// <summary>
    /// Gets an atomic container for a nested atomic property accessed via expression.
    /// </summary>
    /// <typeparam name="TI">The type of the nested atomic value.</typeparam>
    /// <param name="ex">The expression to access the nested atomic value.</param>
    /// <returns>An atomic container for the specified nested atomic value.</returns>
    public IAtomicContainer<TI> AtAtomic<TI>(Expression<Func<Dictionary<TKey, TValue>, TI>> ex)
    {
        return At<IAtomicContainer<TI>>(ex);
    }

    /// <summary>
    /// Adds a key-value pair to the dictionary.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="item">The value to add.</param>
    public void Add(TKey key, TValue item)
    {
        using (Mute())
            AddInternal(key, item);
        HasBeenTouched = true;
        NotifyChanged();
    }

    /// <summary>
    /// Removes a key-value pair from the dictionary.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    public void Remove(TKey key)
    {
        using (Mute())
            RemoveInternal(key);
        HasBeenTouched = true;
        NotifyChanged();
    }

    /// <summary>
    /// Gets a nested state container at the specified expression path.
    /// </summary>
    /// <typeparam name="TX">The type of the nested state container.</typeparam>
    /// <param name="ex">The expression to resolve the path.</param>
    /// <returns>The nested state container.</returns>
    private TX At<TX>(LambdaExpression ex)
        where TX : ITrackedState
    {
        try
        {
            var key = ResolveKey(ex);
            if (!_states.ContainsKey(key))
                throw new IndexOutOfRangeException($"There's no item in container with key {key}");

            return (TX)_states[key].Ref;
        }
        catch (Exception e)
        {
            this.Error(e);
            throw;
        }
    }

    /// <summary>
    /// Creates the current value of the dictionary from the internal state references.
    /// </summary>
    /// <returns>A dictionary containing the current values of all key-value pairs.</returns>
    private Dictionary<TKey, TValue> CreateValue()
    {
        var value = new Dictionary<TKey, TValue>();

        foreach (var (key, state) in _states)
            value[key] = state.Ref.Value;

        return value;
    }

    /// <summary>
    /// Resolves the dictionary key from a lambda expression.
    /// </summary>
    /// <param name="ex">The lambda expression to resolve.</param>
    /// <returns>The resolved dictionary key.</returns>
    private TKey ResolveKey(LambdaExpression ex)
    {
        if (
            ex.Body is MethodCallExpression { Method.IsSpecialName: true } body
            && body.Method.ReturnType == typeof(TValue)
        )
        {
            var parameters = body.Method.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(TKey))
            {
                var value = Expression.Lambda(body.Arguments.ElementAt(0)).Compile().DynamicInvoke();
                if (value is TKey key)
                    return key;
            }
        }

        throw new ArgumentException($"{ex} is not a valid dictionary index expression");
    }

    /// <summary>
    /// Adds a key-value pair to the internal state collection.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="item">The value to add.</param>
    private void AddInternal(TKey key, TValue item)
    {
        var state = (IValueTrackedState<TValue>)Factory.Invoke(_stateFactory, [item])!;
        _states[key] = new StateReference(state, state.Changed.Subscribe(_ => NotifyChanged()));
    }

    /// <summary>
    /// Removes a key-value pair from the internal state collection.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    private void RemoveInternal(TKey key)
    {
        _states[key].Subscription.Dispose();
        _states.Remove(key);
    }

    /// <summary>
    /// Represents a reference to a state with its change notification subscription.
    /// </summary>
    private class StateReference
    {
        /// <summary>
        /// Gets the state reference.
        /// </summary>
        public IValueTrackedState<TValue> Ref { get; }

        /// <summary>
        /// Gets the subscription for change notifications.
        /// </summary>
        public IDisposable Subscription { get; }

        /// <summary>
        /// Initializes a new instance of the StateReference class.
        /// </summary>
        /// <param name="ref">The state reference.</param>
        /// <param name="subscription">The change notification subscription.</param>
        public StateReference(IValueTrackedState<TValue> @ref, IDisposable subscription)
        {
            Ref = @ref;
            Subscription = subscription;
        }

        /// <summary>
        /// Returns a string representation of the state reference.
        /// </summary>
        /// <returns>A friendly name of the state type.</returns>
        public override string ToString() => Ref.GetType().FriendlyName();
    }
}
