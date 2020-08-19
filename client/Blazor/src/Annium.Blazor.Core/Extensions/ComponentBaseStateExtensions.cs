using System;
using System.Reflection;
using Annium.Components.State.Core;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Core.Extensions
{
    public static class ComponentBaseStateExtensions
    {
        private static readonly MethodInfo StateHasChanged = typeof(ComponentBase)
            .GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly object[] EmptyArgs = Array.Empty<object>();

        public static IDisposable ObserveState(this ComponentBase component) =>
            State.Observe(component, () => StateHasChanged.Invoke(component, EmptyArgs));

        public static IDisposable ObserveState<T>(this ComponentBase component, T target)
            where T : notnull =>
            State.Observe(target, () => StateHasChanged.Invoke(component, EmptyArgs));
    }
}