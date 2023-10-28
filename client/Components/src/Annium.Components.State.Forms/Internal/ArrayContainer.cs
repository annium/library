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

internal class ArrayContainer<T> : ObservableState, IArrayContainer<T>, ILogSubject
    where T : notnull, new()
{
    private static MethodInfo Factory { get; } = StateFactoryResolver.ResolveFactory(typeof(T));
    public List<T> Value => CreateValue();
    public bool HasChanged => !Value.IsShallowEqual(_initialValue, _mapper);
    public bool HasBeenTouched => _hasBeenTouched || _states.Any(x => x.Ref.HasBeenTouched);
    public IReadOnlyList<ITrackedState> Children => _states.Select(x => x.Ref).ToArray();
    public ILogger Logger { get; }
    private readonly IStateFactory _stateFactory;
    private readonly IMapper _mapper;
    private readonly IList<StateReference> _states = new List<StateReference>();
    private List<T> _initialValue;
    private bool _hasBeenTouched;

    public ArrayContainer(List<T> initialValue, IStateFactory stateFactory, IMapper mapper, ILogger logger)
    {
        _initialValue = initialValue;
        _stateFactory = stateFactory;
        _mapper = mapper;
        Logger = logger;
        Init(_initialValue);
    }

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

        _hasBeenTouched = false;

        NotifyChanged();
    }

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
            _hasBeenTouched = true;
            NotifyChanged();
        }

        return changed;
    }

    public void Reset() => Init(_initialValue);

    public bool IsStatus(params Status[] statuses)
    {
        foreach (var state in _states)
            if (!state.Ref.IsStatus(statuses))
                return false;

        return true;
    }

    public bool HasStatus(params Status[] statuses)
    {
        foreach (var state in _states)
            if (state.Ref.HasStatus(statuses))
                return true;

        return false;
    }

    public IObjectContainer<TI> AtObject<TI>(Expression<Func<List<T>, TI>> ex)
        where TI : notnull, new()
    {
        return At<IObjectContainer<TI>>(ex);
    }

    public IArrayContainer<TI> AtArray<TI>(Expression<Func<List<T>, List<TI>>> ex)
        where TI : notnull, new()
    {
        return At<IArrayContainer<TI>>(ex);
    }

    public IMapContainer<TK, TV> AtMap<TK, TV>(Expression<Func<List<T>, Dictionary<TK, TV>>> ex)
        where TK : notnull
        where TV : notnull, new()
    {
        return At<IMapContainer<TK, TV>>(ex);
    }

    public IAtomicContainer<TI> AtAtomic<TI>(Expression<Func<List<T>, TI>> ex)
    {
        return At<IAtomicContainer<TI>>(ex);
    }

    public void Add(T item)
    {
        using (Mute())
            AddInternal(_states.Count, item);
        _hasBeenTouched = true;
        NotifyChanged();
    }

    public void Insert(int index, T item)
    {
        using (Mute())
            AddInternal(index, item);
        _hasBeenTouched = true;
        NotifyChanged();
    }

    public void RemoveAt(int index)
    {
        using (Mute())
            _states.RemoveAt(index);
        _hasBeenTouched = true;
        NotifyChanged();
    }

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

    private List<T> CreateValue()
    {
        var value = new List<T>(_states.Count);

        foreach (var state in _states)
            value.Add(state.Ref.Value);

        return value;
    }

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

    private void AddInternal(int index, T item)
    {
        var state = (IValueTrackedState<T>)Factory.Invoke(_stateFactory, new[] { (object)item })!;
        _states.Insert(index, new StateReference(state, state.Changed.Subscribe(_ => NotifyChanged())));
    }

    private void RemoveInternal(int index)
    {
        _states[index].Subscription.Dispose();
        _states.RemoveAt(index);
    }

    private class StateReference
    {
        public IValueTrackedState<T> Ref { get; }
        public IDisposable Subscription { get; }

        public StateReference(IValueTrackedState<T> @ref, IDisposable subscription)
        {
            Ref = @ref;
            Subscription = subscription;
        }

        public override string ToString() => Ref.GetType().FriendlyName();
    }
}
