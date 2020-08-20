using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Mapper;
using Annium.Core.Reflection;
using NodaTime;

namespace Annium.Components.State.Forms.Internal
{
    internal class StateFactory : IStateFactory
    {
        private static readonly MethodInfo[] Factories = typeof(IStateFactory)
            .GetMethods()
            .Where(xx => xx.Name == nameof(IStateFactory.Create))
            .ToArray();

        internal static MethodInfo ResolveFactory(Type type)
        {
            var exactCandidate = Factories.SingleOrDefault(x => x.GetParameters().Single().ParameterType == type);
            if (exactCandidate != null)
                return exactCandidate;

            var inferCandidate = Factories.First(xx => type.GetTargetImplementation(xx.GetParameters().Single().ParameterType) != null);
            var parameterType = inferCandidate.GetParameters().Single().ParameterType;

            if (parameterType.IsGenericParameter)
                return inferCandidate.MakeGenericMethod(type);

            var args = parameterType.ResolveGenericArgumentsByImplementation(type);

            return inferCandidate.MakeGenericMethod(args);
        }

        private readonly IMapper _mapper;

        public StateFactory(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IAtomicContainer<sbyte> Create(sbyte initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<short> Create(short initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<int> Create(int initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<long> Create(long initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<byte> Create(byte initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<ushort> Create(ushort initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<uint> Create(uint initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<ulong> Create(ulong initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<decimal> Create(decimal initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<float> Create(float initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<double> Create(double initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<string> Create(string initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<bool> Create(bool initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<DateTime> Create(DateTime initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<DateTimeOffset> Create(DateTimeOffset initialValue) => CreateAtomic(initialValue);

        public IAtomicContainer<Instant> Create(Instant initialValue) => CreateAtomic(initialValue);

        public IMapContainer<TKey, TValue> Create<TKey, TValue>(IDictionary<TKey, TValue> initialValue) where TKey : notnull where TValue : notnull, new() =>
            new MapContainer<TKey, TValue>(this, initialValue.ToDictionary(x => x.Key, x => x.Value), _mapper);

        public IMapContainer<TKey, TValue> Create<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> initialValue) where TKey : notnull where TValue : notnull, new() =>
            new MapContainer<TKey, TValue>(this, initialValue, _mapper);

        public IMapContainer<TKey, TValue> Create<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> initialValue) where TKey : notnull where TValue : notnull, new() =>
            new MapContainer<TKey, TValue>(this, initialValue.ToDictionary(x => x.Key, x => x.Value), _mapper);

        public IArrayContainer<T> Create<T>(IEnumerable<T> initialValue) where T : notnull, new() => new ArrayContainer<T>(this, initialValue, _mapper);

        public IObjectContainer<T> Create<T>(T initialValue) where T : notnull, new() => new ObjectContainer<T>(this, initialValue);

        private IAtomicContainer<T> CreateAtomic<T>(T defaultValue)
            where T : IEquatable<T>
        {
            return new AtomicContainer<T>(defaultValue);
        }
    }
}