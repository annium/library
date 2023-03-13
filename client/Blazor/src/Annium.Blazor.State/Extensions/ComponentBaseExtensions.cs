using System;
using System.Collections.Generic;
using System.Reflection;
using Annium.Components.State;
using Annium.Components.State.Core;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.State.Extensions;

public static class ComponentBaseExtensions
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

    public static IDisposable ObserveStates(this ComponentBase component) =>
        StateObserver.ObserveObject(component, () => StateHasChanged.Invoke(component, EmptyArgs));
}