using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Reflection;

namespace Annium.Components.State.Core
{
    public static class State
    {
        private static readonly object[] EmptyArgs = Array.Empty<object>();

        private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<Func<object, IObservableState>>> Observables =
            new ConcurrentDictionary<Type, IReadOnlyCollection<Func<object, IObservableState>>>();

        public static IDisposable Observe<T>(T target, Action handleChange)
            where T : notnull
        {
            var observables = Observables.GetOrAdd(target.GetType(), DiscoverObservables);

            return new ObserverDisposer(observables.Select(
                x => x(target).Changed
                    .Subscribe(_ => handleChange())
            ).ToArray());
        }

        private static IReadOnlyCollection<Func<object, IObservableState>> DiscoverObservables(Type type)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var accessors = new List<Func<object, IObservableState>>();

            var properties = type.GetProperties(flags)
                .Where(x => x.PropertyType.IsDerivedFrom(typeof(IObservableState)))
                .ToArray();
            foreach (var property in properties)
                accessors.Add(instance => (IObservableState) property.GetMethod.Invoke(instance, EmptyArgs));

            var fields = type.GetFields(flags)
                .Where(x => x.FieldType.IsDerivedFrom(typeof(IObservableState)))
                .ToArray();
            foreach (var field in fields)
                accessors.Add(instance => (IObservableState) field.GetValue(instance));

            return accessors;
        }

        private class ObserverDisposer : IDisposable
        {
            private IReadOnlyCollection<IDisposable> _disposables;

            public ObserverDisposer(IReadOnlyCollection<IDisposable> disposables)
            {
                _disposables = disposables;
            }

            public void Dispose()
            {
                foreach (var disposable in _disposables)
                    disposable.Dispose();
                _disposables = default!;
            }
        }
    }
}