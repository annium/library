using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Annium.Blazor.Charts.Internal.Data.Comparers;

namespace Annium.Blazor.Charts.Data.Comparers;

public static class ItemComparer
{
    private static readonly ConcurrentDictionary<Type, object> Comparers = new();

    public static IComparer<T> For<T>(Func<T, T, int> compare) =>
        (IComparer<T>) Comparers.GetOrAdd(
            typeof(T),
            static (_, comp) => new ItemComparer<T>(comp),
            compare
        );
}