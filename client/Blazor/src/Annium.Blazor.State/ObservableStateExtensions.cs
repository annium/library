using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Components.State.Core;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.State;

public static class ObservableStateExtensions
{
    private static readonly MethodInfo StateHasChanged = typeof(ComponentBase).GetMethod(nameof(StateHasChanged), BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"Failed to discover {nameof(StateHasChanged)} on {typeof(ObservableState).FriendlyName()}");

    private static readonly object[] EmptyArgs = Array.Empty<object>();

    public static IDisposable Notify<T>(this T state, ComponentBase component)
        where T : IObservableState =>
        state.Notify(_ => StateHasChanged.Invoke(component, EmptyArgs));

    public static IEnumerable<IDisposable> Notify<T>(this IEnumerable<T> states, ComponentBase component)
        where T : IObservableState =>
        states.Notify(_ => StateHasChanged.Invoke(component, EmptyArgs));

    public static IDisposable Notify<T>(this T state, Action<T> handle)
        where T : IObservableState =>
        state.Changed.Subscribe(_ => handle(state));

    public static IEnumerable<IDisposable> Notify<T>(this IEnumerable<T> states, Action<T> handle)
        where T : IObservableState
    {
        return states.Select(x => x.Changed.Subscribe(_ => handle(x)));
    }
}