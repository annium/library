using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Components.State.Core;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.State;

/// <summary>
/// Extension methods for integrating observable state objects with Blazor components
/// </summary>
public static class ObservableStateExtensions
{
    /// <summary>
    /// Name of the StateHasChanged method on ComponentBase
    /// </summary>
    private const string StateHasChangedMethodName = "StateHasChanged";

    /// <summary>
    /// Cached reflection reference to the StateHasChanged method on ComponentBase
    /// </summary>
    private static readonly MethodInfo _stateHasChanged =
        typeof(ComponentBase).GetMethod(StateHasChangedMethodName, BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException(
            $"Failed to discover {StateHasChangedMethodName} on {typeof(ObservableState).FriendlyName()}"
        );

    /// <summary>
    /// Empty arguments array for method invocation
    /// </summary>
    private static readonly object[] _emptyArgs = [];

    /// <summary>
    /// Sets up automatic component re-rendering when the observable state changes
    /// </summary>
    /// <typeparam name="T">The type of observable state</typeparam>
    /// <param name="state">The observable state to monitor</param>
    /// <param name="component">The Blazor component to notify</param>
    /// <returns>A disposable subscription that can be used to stop notifications</returns>
    public static IDisposable Notify<T>(this T state, ComponentBase component)
        where T : IObservableState => state.Notify(_ => _stateHasChanged.Invoke(component, _emptyArgs));

    /// <summary>
    /// Sets up automatic component re-rendering when any of the observable states change
    /// </summary>
    /// <typeparam name="T">The type of observable state</typeparam>
    /// <param name="states">The collection of observable states to monitor</param>
    /// <param name="component">The Blazor component to notify</param>
    /// <returns>A collection of disposable subscriptions that can be used to stop notifications</returns>
    public static IEnumerable<IDisposable> Notify<T>(this IEnumerable<T> states, ComponentBase component)
        where T : IObservableState => states.Notify(_ => _stateHasChanged.Invoke(component, _emptyArgs));

    /// <summary>
    /// Sets up a custom notification handler when the observable state changes
    /// </summary>
    /// <typeparam name="T">The type of observable state</typeparam>
    /// <param name="state">The observable state to monitor</param>
    /// <param name="handle">The action to execute when the state changes, receiving the state as a parameter</param>
    /// <returns>A disposable subscription that can be used to stop notifications</returns>
    public static IDisposable Notify<T>(this T state, Action<T> handle)
        where T : IObservableState => state.Changed.Subscribe(_ => handle(state));

    /// <summary>
    /// Sets up custom notification handlers when any of the observable states change
    /// </summary>
    /// <typeparam name="T">The type of observable state</typeparam>
    /// <param name="states">The collection of observable states to monitor</param>
    /// <param name="handle">The action to execute when a state changes, receiving the changed state as a parameter</param>
    /// <returns>A collection of disposable subscriptions that can be used to stop notifications</returns>
    public static IEnumerable<IDisposable> Notify<T>(this IEnumerable<T> states, Action<T> handle)
        where T : IObservableState
    {
        return states.Select(x => x.Changed.Subscribe(_ => handle(x)));
    }

    /// <summary>
    /// Sets up a parameterless notification handler when the observable state changes
    /// </summary>
    /// <typeparam name="T">The type of observable state</typeparam>
    /// <param name="state">The observable state to monitor</param>
    /// <param name="handle">The action to execute when the state changes</param>
    /// <returns>A disposable subscription that can be used to stop notifications</returns>
    public static IDisposable Notify<T>(this T state, Action handle)
        where T : IObservableState => state.Changed.Subscribe(_ => handle());

    /// <summary>
    /// Sets up parameterless notification handlers when any of the observable states change
    /// </summary>
    /// <typeparam name="T">The type of observable state</typeparam>
    /// <param name="states">The collection of observable states to monitor</param>
    /// <param name="handle">The action to execute when any state changes</param>
    /// <returns>A collection of disposable subscriptions that can be used to stop notifications</returns>
    public static IEnumerable<IDisposable> Notify<T>(this IEnumerable<T> states, Action handle)
        where T : IObservableState
    {
        return states.Select(x => x.Changed.Subscribe(_ => handle()));
    }
}
