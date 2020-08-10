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
    internal class ArrayContainer<T> : ObservableContainer, IArrayContainer<T>
        where T : notnull, new()
    {
        private static MethodInfo Factory { get; } = StateFactory.ResolveFactory(typeof(T));
        public T[] Value => CreateValue();
        public bool HasChanged => !Value.IsShallowEqual(_initialValue, _mapper);
        public bool HasBeenTouched => _hasBeenTouched || _states.Any(x => x.Ref.HasBeenTouched);
        private readonly IStateFactory _stateFactory;
        private readonly IEnumerable<T> _initialValue;
        private readonly IMapper _mapper;
        private readonly IList<StateReference> _states = new List<StateReference>();
        private bool _hasBeenTouched;

        public ArrayContainer(
            IStateFactory stateFactory,
            IEnumerable<T> initialValue,
            IMapper mapper
        )
        {
            _stateFactory = stateFactory;
            _initialValue = initialValue;
            _mapper = mapper;
            Reset();
        }

        public bool Set(T[] value)
        {
            var changed = false;

            var updated = Math.Min(_states.Count, value.Length);
            for (int i = 0; i < updated; i++)
                changed = _states[i].Ref.Set(value[i]) || changed;

            var added = Math.Max(value.Length - _states.Count, 0) + updated;
            for (int i = updated; i < added; i++)
            {
                AddInternal(_states.Count, value[i]);
                changed = true;
            }

            var removed = Math.Max(_states.Count - value.Length, 0) + updated;
            for (int i = updated; i < removed; i++)
            {
                RemoveInternal(i);
                changed = true;
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
            foreach (var item in _initialValue)
            {
                var state = (IState<T>) Factory.Invoke(_stateFactory, new[] { (object) item });
                _states.Add(new StateReference(state, state.Changed.Subscribe(_ => OnChanged())));
            }

            _hasBeenTouched = false;
            OnChanged();
        }

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

        public IArrayContainer<TI> At<TI>(Expression<Func<T[], IEnumerable<TI>>> ex) where TI : notnull, new() => At<IArrayContainer<TI>>(ex);
        public IMapContainer<TK, TV> At<TK, TV>(Expression<Func<T[], IEnumerable<KeyValuePair<TK, TV>>>> ex) where TK : notnull where TV : notnull, new() => At<IMapContainer<TK, TV>>(ex);
        public IAtomicContainer<sbyte> At(Expression<Func<T[], sbyte>> ex) => At<IAtomicContainer<sbyte>>(ex);
        public IAtomicContainer<short> At(Expression<Func<T[], short>> ex) => At<IAtomicContainer<short>>(ex);
        public IAtomicContainer<int> At(Expression<Func<T[], int>> ex) => At<IAtomicContainer<int>>(ex);
        public IAtomicContainer<long> At(Expression<Func<T[], long>> ex) => At<IAtomicContainer<long>>(ex);
        public IAtomicContainer<byte> At(Expression<Func<T[], byte>> ex) => At<IAtomicContainer<byte>>(ex);
        public IAtomicContainer<ushort> At(Expression<Func<T[], ushort>> ex) => At<IAtomicContainer<ushort>>(ex);
        public IAtomicContainer<uint> At(Expression<Func<T[], uint>> ex) => At<IAtomicContainer<uint>>(ex);
        public IAtomicContainer<ulong> At(Expression<Func<T[], ulong>> ex) => At<IAtomicContainer<ulong>>(ex);
        public IAtomicContainer<decimal> At(Expression<Func<T[], decimal>> ex) => At<IAtomicContainer<decimal>>(ex);
        public IAtomicContainer<float> At(Expression<Func<T[], float>> ex) => At<IAtomicContainer<float>>(ex);
        public IAtomicContainer<double> At(Expression<Func<T[], double>> ex) => At<IAtomicContainer<double>>(ex);
        public IAtomicContainer<string> At(Expression<Func<T[], string>> ex) => At<IAtomicContainer<string>>(ex);
        public IAtomicContainer<bool> At(Expression<Func<T[], bool>> ex) => At<IAtomicContainer<bool>>(ex);
        public IAtomicContainer<DateTime> At(Expression<Func<T[], DateTime>> ex) => At<IAtomicContainer<DateTime>>(ex);
        public IAtomicContainer<DateTimeOffset> At(Expression<Func<T[], DateTimeOffset>> ex) => At<IAtomicContainer<DateTimeOffset>>(ex);
        public IAtomicContainer<Instant> At(Expression<Func<T[], Instant>> ex) => At<IAtomicContainer<Instant>>(ex);
        public IObjectContainer<TI> At<TI>(Expression<Func<T[], TI>> ex) where TI : notnull, new() => At<IObjectContainer<TI>>(ex);

        public void Add(T item)
        {
            AddInternal(_states.Count, item);
            _hasBeenTouched = true;
            OnChanged();
        }

        public void Insert(int index, T item)
        {
            AddInternal(index, item);
            _hasBeenTouched = true;
            OnChanged();
        }

        public void RemoveAt(int index)
        {
            _states.RemoveAt(index);
            _hasBeenTouched = true;
            OnChanged();
        }

        private TX At<TX>(LambdaExpression ex) where TX : IState
        {
            var index = ResolveIndex(ex);
            if (index < 0 || index >= _states.Count)
                throw new IndexOutOfRangeException($"There's no item in container with index {index}");

            return (TX) _states[index].Ref;
        }

        private T[] CreateValue()
        {
            var value = new List<T>();

            foreach (var state in _states)
                value.Add(state.Ref.Value);

            return value.ToArray();
        }

        private int ResolveIndex(LambdaExpression ex)
        {
            if (ex.Body is BinaryExpression body && body.NodeType == ExpressionType.ArrayIndex)
            {
                if (body.Right is ConstantExpression constant && constant.Value?.GetType() == typeof(int))
                    return (int) constant.Value;

                if (body.Right is MemberExpression member && member.Expression is ConstantExpression)
                {
                    var value = Expression.Lambda(body.Right).Compile().DynamicInvoke();
                    if (value is int index)
                        return index;
                }
            }

            throw new ArgumentException($"{ex} is not a valid array index expression");
        }

        private void AddInternal(int index, T item)
        {
            var state = (IState<T>) Factory.Invoke(_stateFactory, new[] { (object) item });
            _states.Insert(index, new StateReference(state, state.Changed.Subscribe(_ => OnChanged())));
        }

        private void RemoveInternal(int index)
        {
            _states[index].Subscription.Dispose();
            _states.RemoveAt(index);
        }

        private class StateReference
        {
            public IState<T> Ref { get; }
            public IDisposable Subscription { get; }

            public StateReference(
                IState<T> @ref,
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