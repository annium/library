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
/// Represents a state container for managing arrays of items with change tracking, validation, and nested property access.
/// </summary>
/// <typeparam name="T">The type of items in the array, must be non-null and have a parameterless constructor.</typeparam>
internal class ArrayContainer<T> : ObservableState, IArrayContainer<T>, ILogSubject
    where T : notnull, new()
{
    /// <summary>
    /// Gets the factory method for creating state instances of type T.
    /// </summary>
    private static MethodInfo Factory { get; } = StateFactoryResolver.ResolveFactory(typeof(T));

    /// <summary>
    /// Gets the current value of the array as a list.
    /// </summary>
    public List<T> Value => CreateValue();

    /// <summary>
    /// Gets a value indicating whether the array has changed from its initial value.
    /// </summary>
    public bool HasChanged => !Value.IsShallowEqual(_initialValue, _mapper);

    /// <summary>
    /// Gets a value indicating whether the array or any of its items have been touched (modified).
    /// </summary>
    public bool HasBeenTouched
    {
        get => field || _states.Any(x => x.Ref.HasBeenTouched);
        private set;
    }

    /// <summary>
    /// Gets the child states representing individual items in the array.
    /// </summary>
    public IReadOnlyList<ITrackedState> Children => _states.Select(x => x.Ref).ToArray();

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
    /// The collection of state references for individual array items.
    /// </summary>
    private readonly IList<StateReference> _states = new List<StateReference>();

    /// <summary>
    /// The initial value of the array.
    /// </summary>
    private List<T> _initialValue;

    /// <summary>
    /// Initializes a new instance of the ArrayContainer class.
    /// </summary>
    /// <param name="initialValue">The initial value of the array.</param>
    /// <param name="stateFactory">The state factory for creating new state instances.</param>
    /// <param name="mapper">The mapper for value comparison and transformation.</param>
    /// <param name="logger">The logger instance.</param>
    public ArrayContainer(List<T> initialValue, IStateFactory stateFactory, IMapper mapper, ILogger logger)
    {
        _initialValue = initialValue;
        _stateFactory = stateFactory;
        _mapper = mapper;
        Logger = logger;
        Init(_initialValue);
    }

    /// <summary>
    /// Initializes the array container with a new value, resetting the touched state.
    /// </summary>
    /// <param name="value">The new value to initialize with.</param>
    public void Init(List<T> value)
    {
        _initialValue = value;
        using (Mute())
        {
            var updated = Math.Min(_states.Count, value.Count);
            for (int i = 0; i < updated; i++)
                _states[i].Ref.Init(value[i]);

            var added = Math.Max(value.Count - _states.Count, 0) + updated;
            for (int i = updated; i < added; i++)
                AddInternal(_states.Count, value[i]);

            var removed = Math.Max(_states.Count - value.Count, 0) + updated;
            for (int i = updated; i < removed; i++)
                RemoveInternal(i);
        }

        HasBeenTouched = false;

        NotifyChanged();
    }

    /// <summary>
    /// Sets a new value for the array, marking it as touched if changes occur.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    /// <returns>True if the value was changed, false otherwise.</returns>
    public bool Set(List<T> value)
    {
        var changed = false;

        using (Mute())
        {
            var updated = Math.Min(_states.Count, value.Count);
            for (int i = 0; i < updated; i++)
                changed = _states[i].Ref.Set(value[i]) || changed;

            var added = Math.Max(value.Count - _states.Count, 0) + updated;
            for (int i = updated; i < added; i++)
            {
                AddInternal(_states.Count, value[i]);
                changed = true;
            }

            var removed = Math.Max(_states.Count - value.Count, 0) + updated;
            for (int i = updated; i < removed; i++)
            {
                RemoveInternal(i);
                changed = true;
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
    /// Resets the array to its initial value.
    /// </summary>
    public void Reset() => Init(_initialValue);

    /// <summary>
    /// Checks if all items in the array have any of the specified statuses.
    /// </summary>
    /// <param name="statuses">The statuses to check for.</param>
    /// <returns>True if all items have any of the specified statuses, false otherwise.</returns>
    public bool IsStatus(params Status[] statuses)
    {
        foreach (var state in _states)
            if (!state.Ref.IsStatus(statuses))
                return false;

        return true;
    }

    /// <summary>
    /// Checks if any item in the array has any of the specified statuses.
    /// </summary>
    /// <param name="statuses">The statuses to check for.</param>
    /// <returns>True if any item has any of the specified statuses, false otherwise.</returns>
    public bool HasStatus(params Status[] statuses)
    {
        foreach (var state in _states)
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
    public IObjectContainer<TI> AtObject<TI>(Expression<Func<List<T>, TI>> ex)
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
    public IArrayContainer<TI> AtArray<TI>(Expression<Func<List<T>, List<TI>>> ex)
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
    public IMapContainer<TK, TV> AtMap<TK, TV>(Expression<Func<List<T>, Dictionary<TK, TV>>> ex)
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
    public IAtomicContainer<TI> AtAtomic<TI>(Expression<Func<List<T>, TI>> ex)
    {
        return At<IAtomicContainer<TI>>(ex);
    }

    /// <summary>
    /// Adds an item to the end of the array.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        using (Mute())
            AddInternal(_states.Count, item);
        HasBeenTouched = true;
        NotifyChanged();
    }

    /// <summary>
    /// Inserts an item at the specified index in the array.
    /// </summary>
    /// <param name="index">The index at which to insert the item.</param>
    /// <param name="item">The item to insert.</param>
    public void Insert(int index, T item)
    {
        using (Mute())
            AddInternal(index, item);
        HasBeenTouched = true;
        NotifyChanged();
    }

    /// <summary>
    /// Removes the item at the specified index from the array.
    /// </summary>
    /// <param name="index">The index of the item to remove.</param>
    public void RemoveAt(int index)
    {
        using (Mute())
            _states.RemoveAt(index);
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
            var index = ResolveIndex(ex);
            if (index < 0 || index >= _states.Count)
                throw new IndexOutOfRangeException($"There's no item in container with index {index}");

            return (TX)_states[index].Ref;
        }
        catch (Exception e)
        {
            this.Error(e);
            throw;
        }
    }

    /// <summary>
    /// Creates the current value of the array from the internal state references.
    /// </summary>
    /// <returns>A list containing the current values of all items.</returns>
    private List<T> CreateValue()
    {
        var value = new List<T>(_states.Count);

        foreach (var state in _states)
            value.Add(state.Ref.Value);

        return value;
    }

    /// <summary>
    /// Resolves the array index from a lambda expression.
    /// </summary>
    /// <param name="ex">The lambda expression to resolve.</param>
    /// <returns>The resolved array index.</returns>
    private int ResolveIndex(LambdaExpression ex)
    {
        if (
            ex.Body is MethodCallExpression
            {
                NodeType: ExpressionType.Call,
                Method.IsSpecialName: true,
                Arguments.Count: 1
            } body
        )
        {
            var arg = body.Arguments.ElementAt(0);
            if (arg is ConstantExpression constant && constant.Value?.GetType() == typeof(int))
                return (int)constant.Value;

            if (arg is MemberExpression { Expression: ConstantExpression })
            {
                var value = Expression.Lambda(arg).Compile().DynamicInvoke();
                if (value is int index)
                    return index;
            }
        }

        throw new ArgumentException($"{ex} is not a valid array index expression");
    }

    /// <summary>
    /// Adds an item to the internal state collection at the specified index.
    /// </summary>
    /// <param name="index">The index at which to add the item.</param>
    /// <param name="item">The item to add.</param>
    private void AddInternal(int index, T item)
    {
        var state = (IValueTrackedState<T>)Factory.Invoke(_stateFactory, [item])!;
        _states.Insert(index, new StateReference(state, state.Changed.Subscribe(_ => NotifyChanged())));
    }

    /// <summary>
    /// Removes an item from the internal state collection at the specified index.
    /// </summary>
    /// <param name="index">The index of the item to remove.</param>
    private void RemoveInternal(int index)
    {
        _states[index].Subscription.Dispose();
        _states.RemoveAt(index);
    }

    /// <summary>
    /// Represents a reference to a state with its change notification subscription.
    /// </summary>
    private class StateReference
    {
        /// <summary>
        /// Gets the state reference.
        /// </summary>
        public IValueTrackedState<T> Ref { get; }

        /// <summary>
        /// Gets the subscription for change notifications.
        /// </summary>
        public IDisposable Subscription { get; }

        /// <summary>
        /// Initializes a new instance of the StateReference class.
        /// </summary>
        /// <param name="ref">The state reference.</param>
        /// <param name="subscription">The change notification subscription.</param>
        public StateReference(IValueTrackedState<T> @ref, IDisposable subscription)
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
