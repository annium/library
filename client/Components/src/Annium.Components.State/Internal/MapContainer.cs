using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions;
using Annium.Extensions.Primitives;
using NodaTime;

namespace Annium.Components.State.Internal
{
    internal class MapContainer<TKey, TValue> : ObservableContainer, IMapContainer<TKey, TValue>
        where TKey : notnull
        where TValue : notnull, new()
    {
        private static MethodInfo Factory { get; } = StateFactory.ResolveFactory(typeof(TValue));
        public IReadOnlyDictionary<TKey, TValue> Value => CreateValue();
        public bool HasChanged => !Value.IsShallowEqual(_initialValue, _mapper);
        public bool HasBeenTouched => _hasBeenTouched || _states.Values.Any(x => x.Ref.HasBeenTouched);
        private readonly IStateFactory _stateFactory;
        private readonly IReadOnlyDictionary<TKey, TValue> _initialValue;
        private readonly IMapper _mapper;
        private readonly IDictionary<TKey, StateReference> _states;
        private bool _hasBeenTouched;

        public MapContainer(
            IStateFactory stateFactory,
            IReadOnlyDictionary<TKey, TValue> initialValue,
            IMapper mapper
        )
        {
            _stateFactory = stateFactory;
            _initialValue = initialValue;
            _mapper = mapper;
            _states = new Dictionary<TKey, StateReference>();
            Reset();
        }

        public bool Set(IReadOnlyDictionary<TKey, TValue> value)
        {
            var changed = false;

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

            if (changed)
            {
                _hasBeenTouched = true;
                OnChanged();
            }

            return changed;
        }

        public void Reset()
        {
            _states.Clear();
            foreach (var (key, item) in _initialValue)
                AddInternal(key, item);

            _hasBeenTouched = false;
            OnChanged();
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

        public IArrayContainer<TI> At<TI>(Expression<Func<IReadOnlyDictionary<TKey, TValue>, IEnumerable<TI>>> ex) where TI : notnull, new() => At<IArrayContainer<TI>>(ex);
        public IMapContainer<TK, TV> At<TK, TV>(Expression<Func<IReadOnlyDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TK, TV>>>> ex) where TK : notnull where TV : notnull, new() => At<IMapContainer<TK, TV>>(ex);
        public IAtomicContainer<sbyte> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, sbyte>> ex) => At<IAtomicContainer<sbyte>>(ex);
        public IAtomicContainer<short> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, short>> ex) => At<IAtomicContainer<short>>(ex);
        public IAtomicContainer<int> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, int>> ex) => At<IAtomicContainer<int>>(ex);
        public IAtomicContainer<long> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, long>> ex) => At<IAtomicContainer<long>>(ex);
        public IAtomicContainer<byte> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, byte>> ex) => At<IAtomicContainer<byte>>(ex);
        public IAtomicContainer<ushort> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, ushort>> ex) => At<IAtomicContainer<ushort>>(ex);
        public IAtomicContainer<uint> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, uint>> ex) => At<IAtomicContainer<uint>>(ex);
        public IAtomicContainer<ulong> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, ulong>> ex) => At<IAtomicContainer<ulong>>(ex);
        public IAtomicContainer<decimal> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, decimal>> ex) => At<IAtomicContainer<decimal>>(ex);
        public IAtomicContainer<float> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, float>> ex) => At<IAtomicContainer<float>>(ex);
        public IAtomicContainer<double> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, double>> ex) => At<IAtomicContainer<double>>(ex);
        public IAtomicContainer<string> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, string>> ex) => At<IAtomicContainer<string>>(ex);
        public IAtomicContainer<bool> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, bool>> ex) => At<IAtomicContainer<bool>>(ex);
        public IAtomicContainer<DateTime> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, DateTime>> ex) => At<IAtomicContainer<DateTime>>(ex);
        public IAtomicContainer<DateTimeOffset> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, DateTimeOffset>> ex) => At<IAtomicContainer<DateTimeOffset>>(ex);
        public IAtomicContainer<Instant> At(Expression<Func<IReadOnlyDictionary<TKey, TValue>, Instant>> ex) => At<IAtomicContainer<Instant>>(ex);
        public IObjectContainer<TI> At<TI>(Expression<Func<IReadOnlyDictionary<TKey, TValue>, TI>> ex) where TI : notnull, new() => At<IObjectContainer<TI>>(ex);

        public void Add(TKey key, TValue item)
        {
            AddInternal(key, item);
            _hasBeenTouched = true;
            OnChanged();
        }

        public void Remove(TKey key)
        {
            RemoveInternal(key);
            _hasBeenTouched = true;
            OnChanged();
        }

        private TX At<TX>(LambdaExpression ex) where TX : IState
        {
            var key = ResolveKey(ex);
            if (!_states.ContainsKey(key))
                throw new IndexOutOfRangeException($"There's no item in container with key {key}");

            return (TX) _states[key].Ref;
        }

        private IReadOnlyDictionary<TKey, TValue> CreateValue()
        {
            var value = new Dictionary<TKey, TValue>();

            foreach (var (key, state) in _states)
                value[key] = state.Ref.Value;

            return value;
        }

        private TKey ResolveKey(LambdaExpression ex)
        {
            if (ex.Body is MethodCallExpression body && body.Method.IsSpecialName && body.Method.ReturnType == typeof(TValue))
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

        private void AddInternal(TKey key, TValue item)
        {
            var state = (IState<TValue>) Factory.Invoke(_stateFactory, new[] { (object) item });
            _states[key] = new StateReference(state, state.Changed.Subscribe(_ => OnChanged()));
        }

        private void RemoveInternal(TKey key)
        {
            _states[key].Subscription.Dispose();
            _states.Remove(key);
        }

        private class StateReference
        {
            public IState<TValue> Ref { get; }
            public IDisposable Subscription { get; }

            public StateReference(
                IState<TValue> @ref,
                IDisposable subscription
            )
            {
                Ref = @ref;
                Subscription = subscription;
            }

            public override string ToString() => Ref.GetType().FriendlyName();
        }
    }
}