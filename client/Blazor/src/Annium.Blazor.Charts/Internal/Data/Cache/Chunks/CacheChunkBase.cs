using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Extensions;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache.Chunks;

internal abstract record CacheChunkBase<T>
{
    private readonly IComparer<T> _comparer;
    public ValueRange<Instant> Range => _range;
    public List<T> Items { get; }
    private readonly ManagedValueRange<Instant> _range;

    protected CacheChunkBase(Instant start, Instant end, IReadOnlyCollection<T> items, IComparer<T> comparer)
    {
        _comparer = comparer;
        _range = ValueRange.Create(start, end);
        Items = items.OrderBy(x => x, comparer).ToList();
        Validate();
    }

    public void Append(CacheChunkBase<T> chunk)
    {
        _range.SetEnd(chunk.Range.End);
        Items.AddRange(chunk.Items);
        Items.Sort(_comparer);
    }

    private void Validate()
    {
        if (Range.Start > Range.End)
            throw new InvalidOperationException($"Invalid chunk: {Range} is invalid");
    }

    public override string ToString() => $"{typeof(T).FriendlyName()}[{Items.Count}] ({Range.S()})";
}
