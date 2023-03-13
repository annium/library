using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Components.State;
using Annium.Components.State.Core;

namespace Annium.Blazor.State.Extensions;

public static class ObservableStateExtensions
{
    private static readonly MethodInfo NotifyChanged = typeof(ObservableState).GetMethod(nameof(NotifyChanged), BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"Failed to discover {nameof(NotifyChanged)} on {typeof(ObservableState).FriendlyName()}");

    private static readonly object[] EmptyArgs = Array.Empty<object>();

    public static IDisposable Notify<T>(this T state, Action<T> handle)
        where T : IObservableState =>
        state.Changed.Subscribe(_ => handle(state));

    public static IEnumerable<IDisposable> Notify<T>(this IEnumerable<T> states, Action<T> handle)
        where T : IObservableState
    {
        return states.Select(x => x.Changed.Subscribe(_ => handle(x)));
    }

    public static IDisposable ObserveStates(this IObservableState state) =>
        StateObserver.ObserveObject(state, () => NotifyChanged.Invoke(state, EmptyArgs));
}