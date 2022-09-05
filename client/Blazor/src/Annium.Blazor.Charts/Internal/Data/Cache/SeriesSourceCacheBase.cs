using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Annium.Blazor.Charts.Domain;
using Annium.Core.Primitives;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

internal abstract class SeriesSourceCacheBase<TChunk, T> : ISeriesSourceCache<T>
    where TChunk : CacheChunkBase<T>
    where T : IComparable<T>
{
    public event Action OnBoundsChange = delegate { };
    public bool IsEmpty => Chunks.Count == 0;
    public ValueRange<Instant> Bounds => _bounds;
    protected Duration Resolution;
    protected IList<TChunk> Chunks { get; } = new List<TChunk>();
    private readonly ManagedValueRange<Instant> _bounds;
    private readonly Func<Instant, Instant, IReadOnlyCollection<T>, TChunk> _createChunk;
    private readonly Func<T, Instant, int> _compare;
    private readonly Func<TChunk, Instant> _getStart;
    private readonly Func<TChunk, Instant> _getEnd;

    protected SeriesSourceCacheBase(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyCollection<T>, TChunk> createChunk,
        Func<T, Instant, int> compare,
        Func<TChunk, Instant> getStart,
        Func<TChunk, Instant> getEnd
    )
    {
        Resolution = resolution;
        _createChunk = createChunk;
        _compare = compare;
        _getStart = getStart;
        _getEnd = getEnd;
        _bounds = ValueRange.Create(NodaConstants.UnixEpoch, NodaConstants.UnixEpoch);
    }

    public bool HasData(Instant start, Instant end)
    {
        var range = ValueRange.Create(start, end);

        foreach (var chunk in Chunks)
            if (chunk.Range.Contains(range, RangeBounds.Both))
                return true;

        return false;
    }

    public IReadOnlyList<T> GetData(Instant start, Instant end)
    {
        var range = ValueRange.Create(start, end);

        foreach (var chunk in Chunks)
            if (chunk.Range.Contains(range, RangeBounds.Both))
                return chunk.Items.Where(x => _compare(x, start) >= 0 && _compare(x, end) <= 0).ToArray();

        return Array.Empty<T>();
    }

    public T? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact)
    {
        foreach (var chunk in Chunks)
            if (chunk.Range.Contains(moment, RangeBounds.Both))
                return GetChunkItem(chunk, moment, match);

        return default;
    }

    public IReadOnlyList<ValueRange<Instant>> GetEmptyRanges(Instant start, Instant end)
    {
        var ranges = new List<ValueRange<Instant>>();

        if (Chunks.Count == 0)
        {
            ranges.Add(ValueRange.Create(start, end));
            return ranges;
        }

        var from = start;

        foreach (var chunk in Chunks)
        {
            if (chunk.Range.Start >= end)
                break;

            if (chunk.Range.Start > from)
                ranges.Add(ValueRange.Create(from, chunk.Range.Start - Resolution));

            from = chunk.Range.End + Resolution;
        }

        if (end > from)
            ranges.Add(ValueRange.Create(from, end));

        return ranges;
    }

    public void AddData(Instant start, Instant end, IReadOnlyCollection<T> data)
    {
        var newChunk = _createChunk(start, end, data);

        if (Chunks.Count == 0)
        {
            Chunks.Add(newChunk);
            SyncBounds();
            PostProcessDataChange();

            return;
        }

        var isAdded = false;
        for (var i = 0; i < Chunks.Count; i++)
        {
            var chunk = Chunks[i];
            if (chunk.Range.Contains(newChunk.Range.Start, RangeBounds.Both) || chunk.Range.Contains(newChunk.Range.End, RangeBounds.Both))
                throw new InvalidOperationException($"New chunk {newChunk.Range} intersects with existing chunk {chunk}");

            if (newChunk.Range.End > chunk.Range.Start)
                continue;

            Chunks.Insert(i, newChunk);
            isAdded = true;
            break;
        }

        if (!isAdded)
            Chunks.Add(newChunk);

        Optimize();
        SyncBounds();
        PostProcessDataChange();
    }

    public void SetResolution(Duration resolution)
    {
        if (resolution == Resolution)
            return;

        Resolution = resolution;
        Chunks.Clear();
        ResetBounds();
    }

    public void Clear()
    {
        Chunks.Clear();
        ResetBounds();
    }

    protected abstract void PostProcessDataChange();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SyncBounds() => SetBounds(_getStart(Chunks[0]), _getEnd(Chunks[^1]));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResetBounds() => SetBounds(NodaConstants.UnixEpoch, NodaConstants.UnixEpoch);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetBounds(Instant start, Instant end)
    {
        if (start == _bounds.Start && end == _bounds.End)
            return;

        _bounds.SetStart(start);
        _bounds.SetEnd(end);
        OnBoundsChange();
    }

    private void Optimize()
    {
        var i = 0;

        while (i < Chunks.Count - 1)
        {
            var current = Chunks[i];
            var next = Chunks[i + 1];

            if (next.Range.Start - current.Range.End == Resolution)
            {
                current.Append(next);
                Chunks.RemoveAt(i + 1);
            }
            else
                i++;
        }
    }

    private T? GetChunkItem(TChunk chunk, Instant moment, LookupMatch match)
    {
        var items = chunk.Items;

        if (_compare(items[0], moment) > 0)
            return match switch
            {
                LookupMatch.NearestRight => items[0],
                _ => default,
            };

        if (_compare(items[^1], moment) < 0)
            return match switch
            {
                LookupMatch.NearestLeft => items[^1],
                _ => default,
            };

        var l = 0;
        var r = items.Count - 1;

        while (l <= r)
        {
            var i = ((r - l) / 2m).FloorInt32().Within(l, r);
            var item = items[i];

            if (_compare(item, moment) < 0)
                l = i + 1;
            else if (_compare(item, moment) > 0)
                r = i - 1;
            else
                return item;
        }

        return match switch
        {
            LookupMatch.NearestLeft => r >= 0 ? items[r] : default,
            LookupMatch.NearestRight => l < items.Count ? items[l] : default,
            _ => default,
        };
    }
}