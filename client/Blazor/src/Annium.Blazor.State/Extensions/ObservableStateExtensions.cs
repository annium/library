using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Components.State.Core;

namespace Annium.Blazor.State.Extensions;

public static class ObservableStateExtensions
{
    public static IDisposable Notify<T>(this T state, Action<T> handle)
        where T : IObservableState =>
        state.Changed.Subscribe(_ => handle(state));

    public static IEnumerable<IDisposable> Notify<T>(this IEnumerable<T> states, Action<T> handle)
        where T : IObservableState
    {
        return states.Select(x => x.Changed.Subscribe(_ => handle(x)));
    }
}