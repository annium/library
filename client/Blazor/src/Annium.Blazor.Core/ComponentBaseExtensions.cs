using System;
using System.Reflection;
using Annium.Components.State.Core;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Core;

public static class ComponentBaseExtensions
{
    private static readonly MethodInfo StateHasChanged = typeof(ComponentBase).GetMethod(nameof(StateHasChanged), BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"Failed to discover {nameof(StateHasChanged)} on {typeof(ObservableState).FriendlyName()}");

    private static readonly object[] EmptyArgs = Array.Empty<object>();

    public static IDisposable ObserveStates(this ComponentBase component) =>
        StateObserver.ObserveObject(component, () => StateHasChanged.Invoke(component, EmptyArgs));
}