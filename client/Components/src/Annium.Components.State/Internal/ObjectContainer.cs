using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Extensions.Primitives;
using NodaTime;

namespace Annium.Components.State.Internal
{
    internal class ObjectContainer<T> : ObservableContainer, IObjectContainer<T>
        where T : notnull, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static PropertyInfo[] Properties { get; }

        // ReSharper disable once StaticMemberInGenericType
        private static IReadOnlyDictionary<PropertyInfo, MethodInfo> Factories { get; }

        static ObjectContainer()
        {
            Properties = typeof(T).GetProperties().Where(x => x.CanRead && x.CanWrite).ToArray();
            Factories = Properties.ToDictionary(x => x, x => StateFactory.ResolveFactory(x.PropertyType));
        }

        public T Value => CreateValue();
        public bool HasChanged => _states.Values.Any(x => x.Ref.HasChanged);
        public bool HasBeenTouched => _states.Values.Any(x => x.Ref.HasBeenTouched);
        public IReadOnlyDictionary<string, IState> Children => _states.ToDictionary(x => x.Key.Name, x => x.Value.Ref);
        private readonly IReadOnlyDictionary<PropertyInfo, StateReference> _states;

        public ObjectContainer(
            IStateFactory stateFactory,
            T initialValue
        )
        {
            var states = new Dictionary<PropertyInfo, StateReference>();
            foreach (var property in Properties)
            {
                var create = Factories[property];
                var @ref = (IState) create.Invoke(stateFactory, new[] { property.GetMethod.Invoke(initialValue, Array.Empty<object>()) });
                @ref.Changed.Subscribe(_ => OnChanged());
                var get = @ref.GetType().GetProperty(nameof(IState<object>.Value)).GetMethod;
                var set = @ref.GetType().GetMethod(nameof(IState<object>.Set));
                states[property] = new StateReference(@ref, get, set);
            }

            _states = states;
        }

        public bool Set(T value)
        {
            var changed = false;
            using (Mute())
            {
                foreach (var property in Properties)
                {
                    var state = _states[property];
                    var propertyValue = property.GetMethod.Invoke(value, Array.Empty<object>());
                    changed = (bool) state.Set.Invoke(state.Ref, new[] { propertyValue }) || changed;
                }
            }

            if (changed)
                OnChanged();

            return changed;
        }

        public void Reset()
        {
            using (Mute())
            {
                foreach (var property in Properties)
                    _states[property].Ref.Reset();
            }

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

        public IArrayContainer<TI> At<TI>(Expression<Func<T, IEnumerable<TI>>> ex) where TI : notnull, new() => At<IArrayContainer<TI>>(ex);
        public IMapContainer<TK, TV> At<TK, TV>(Expression<Func<T, IEnumerable<KeyValuePair<TK, TV>>>> ex) where TK : notnull where TV : notnull, new() => At<IMapContainer<TK, TV>>(ex);
        public IAtomicContainer<sbyte> At(Expression<Func<T, sbyte>> ex) => At<IAtomicContainer<sbyte>>(ex);
        public IAtomicContainer<short> At(Expression<Func<T, short>> ex) => At<IAtomicContainer<short>>(ex);
        public IAtomicContainer<int> At(Expression<Func<T, int>> ex) => At<IAtomicContainer<int>>(ex);
        public IAtomicContainer<long> At(Expression<Func<T, long>> ex) => At<IAtomicContainer<long>>(ex);
        public IAtomicContainer<byte> At(Expression<Func<T, byte>> ex) => At<IAtomicContainer<byte>>(ex);
        public IAtomicContainer<ushort> At(Expression<Func<T, ushort>> ex) => At<IAtomicContainer<ushort>>(ex);
        public IAtomicContainer<uint> At(Expression<Func<T, uint>> ex) => At<IAtomicContainer<uint>>(ex);
        public IAtomicContainer<ulong> At(Expression<Func<T, ulong>> ex) => At<IAtomicContainer<ulong>>(ex);
        public IAtomicContainer<decimal> At(Expression<Func<T, decimal>> ex) => At<IAtomicContainer<decimal>>(ex);
        public IAtomicContainer<float> At(Expression<Func<T, float>> ex) => At<IAtomicContainer<float>>(ex);
        public IAtomicContainer<double> At(Expression<Func<T, double>> ex) => At<IAtomicContainer<double>>(ex);
        public IAtomicContainer<string> At(Expression<Func<T, string>> ex) => At<IAtomicContainer<string>>(ex);
        public IAtomicContainer<bool> At(Expression<Func<T, bool>> ex) => At<IAtomicContainer<bool>>(ex);
        public IAtomicContainer<DateTime> At(Expression<Func<T, DateTime>> ex) => At<IAtomicContainer<DateTime>>(ex);
        public IAtomicContainer<DateTimeOffset> At(Expression<Func<T, DateTimeOffset>> ex) => At<IAtomicContainer<DateTimeOffset>>(ex);
        public IAtomicContainer<Instant> At(Expression<Func<T, Instant>> ex) => At<IAtomicContainer<Instant>>(ex);
        public IObjectContainer<TI> At<TI>(Expression<Func<T, TI>> ex) where TI : notnull, new() => At<IObjectContainer<TI>>(ex);

        private T CreateValue()
        {
            var value = new T();
            foreach (var property in Properties)
            {
                var state = _states[property];
                property.SetMethod.Invoke(value, new[] { state.Get.Invoke(state.Ref, Array.Empty<object>()) });
            }

            return value;
        }

        private TX At<TX>(LambdaExpression ex) where TX : IState => (TX) _states[ResolveProperty(ex)].Ref;

        private PropertyInfo ResolveProperty(LambdaExpression ex)
        {
            if (ex.Body is MemberExpression body && body.Member is PropertyInfo property)
                return property;
            throw new ArgumentException($"{ex} is not a direct property access expression");
        }

        private class StateReference
        {
            public IState Ref { get; }
            public MethodInfo Get { get; }
            public MethodInfo Set { get; }

            public StateReference(
                IState @ref,
                MethodInfo get,
                MethodInfo set
            )
            {
                Ref = @ref;
                Get = get;
                Set = set;
            }

            public override string ToString() => Ref.GetType().FriendlyName();
        }
    }
}