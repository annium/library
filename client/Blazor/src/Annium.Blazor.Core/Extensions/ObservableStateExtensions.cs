using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Components.State.Core;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Core.Extensions;

public static class ObservableStateExtensions
{
    private static readonly MethodInfo StateHasChanged = typeof(ComponentBase)
        .GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly object[] EmptyArgs = Array.Empty<object>();

    public static IDisposable Notify<T>(this T state, Action<T> handle)
        where T : IObservableState =>
        state.Changed.Subscribe(_ => handle(state));

    public static IEnumerable<IDisposable> Notify<T>(this IEnumerable<T> states, Action<T> handle)
        where T : IObservableState
    {
        return states.Select(x => x.Changed.Subscribe(_ => handle(x)));
    }

    public static IDisposable Notify<T>(this T state, ComponentBase component)
        where T : IObservableState =>
        state.Changed.Subscribe(_ => StateHasChanged.Invoke(component, EmptyArgs));

    public static IEnumerable<IDisposable> Notify<T>(this IEnumerable<T> states, ComponentBase component)
        where T : IObservableState
    {
        return states.Select(x => x.Changed.Subscribe(_ => StateHasChanged.Invoke(component, EmptyArgs)));
    }

    public static IDisposable ObserveStates(this ComponentBase component) =>
        State.ObserveObject(component, () => StateHasChanged.Invoke(component, EmptyArgs));
}