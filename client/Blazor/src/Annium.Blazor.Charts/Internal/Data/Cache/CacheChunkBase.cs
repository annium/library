using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Extensions;
using Annium.Core.Primitives;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

internal abstract record CacheChunkBase<T>
    where T : IComparable<T>
{
    public ValueRange<Instant> Range => _range;
    public List<T> Items { get; }
    private readonly ManagedValueRange<Instant> _range;

    protected CacheChunkBase(Instant start, Instant end, IReadOnlyCollection<T> items)
    {
        _range = ValueRange.Create(start, end);
        Items = items.OrderBy(x => x).ToList();
        Validate();
    }

    public void Append(CacheChunkBase<T> chunk)
    {
        _range.SetEnd(chunk.Range.End);
        Items.AddRange(chunk.Items);
    }

    private void Validate()
    {
        if (Range.Start > Range.End)
            throw new InvalidOperationException($"Invalid chunk: {Range} is invalid");
    }

    public override string ToString() => $"{typeof(T).FriendlyName()}[{Items.Count}] ({Range.S()})";
}