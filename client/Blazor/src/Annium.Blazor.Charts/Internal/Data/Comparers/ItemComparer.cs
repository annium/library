using System;
using System.Collections.Generic;

namespace Annium.Blazor.Charts.Internal.Data.Comparers;

internal class ItemComparer<T> : IComparer<T>
{
    private readonly Func<T, T, int> _compare;

    public ItemComparer(Func<T, T, int> compare)
    {
        _compare = compare;
    }

    public int Compare(T? x, T? y)
    {
        if (x is null || y is null)
            throw new ArgumentNullException($"Can't compare null values of {typeof(T).FriendlyName()}");

        return _compare(x, y);
    }
}