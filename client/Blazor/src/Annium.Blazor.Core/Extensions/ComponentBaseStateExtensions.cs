using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Components.State;
using Annium.Core.Reflection;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Core.Extensions
{
    public static class ComponentBaseStateExtensions
    {
        private static readonly MethodInfo StateHasChanged = typeof(ComponentBase)
            .GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly object[] EmptyArgs = Array.Empty<object>();

        private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<Func<ComponentBase, IState>>> Observables =
            new ConcurrentDictionary<Type, IReadOnlyCollection<Func<ComponentBase, IState>>>();

        public static IDisposable ObserveState(this ComponentBase component)
        {
            var observables = Observables.GetOrAdd(component.GetType(), DiscoverObservables);

            return new ObserverDisposer(observables.Select(
                x => x(component).Changed
                    .Subscribe(_ => StateHasChanged.Invoke(component, EmptyArgs))
            ).ToArray());
        }

        private static IReadOnlyCollection<Func<ComponentBase, IState>> DiscoverObservables(Type type)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var accessors = new List<Func<ComponentBase, IState>>();

            var properties = type.GetProperties(flags)
                .Where(x => x.PropertyType.IsDerivedFrom(typeof(IState)))
                .ToArray();
            foreach (var property in properties)
                accessors.Add(component => (IState) property.GetMethod.Invoke(component, EmptyArgs));

            var fields = type.GetFields(flags)
                .Where(x => x.FieldType.IsDerivedFrom(typeof(IState)))
                .ToArray();
            foreach (var field in fields)
                accessors.Add(component => (IState) field.GetValue(component));

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