using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Domain;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data;

internal class SeriesSourceCache<T> : ISeriesSourceCache<T>
    where T : ITimeSeries
{
    public bool IsEmpty => _chunks.Count == 0;
    public ValueRange<Instant> Bounds { get; }
    private readonly IList<Chunk> _chunks = new List<Chunk>();
    private Duration _resolution;
    private readonly SeriesSourceCacheOptions _options;

    public SeriesSourceCache(
        Duration resolution,
        SeriesSourceCacheOptions options
    )
    {
        _resolution = resolution;
        _options = options;
        Bounds = ValueRange.Create(
            () => _chunks.Count > 0 ? _chunks[0].Range.Start : NodaConstants.UnixEpoch,
            () => _chunks.Count > 0 ? _chunks[^1].Range.End : NodaConstants.UnixEpoch
        );
    }

    public bool HasData(Instant start, Instant end)
    {
        var range = ValueRange.Create(start, end);

        foreach (var chunk in _chunks)
            if (chunk.Range.Contains(range, RangeBounds.Both))
                return true;

        return false;
    }

    public IReadOnlyList<T> GetData(Instant start, Instant end)
    {
        var range = ValueRange.Create(start, end);

        foreach (var chunk in _chunks)
            if (chunk.Range.Contains(range, RangeBounds.Both))
                return chunk.Items.Where(x => x.Moment >= start && x.Moment <= end).ToArray();

        return Array.Empty<T>();
    }

    public IReadOnlyList<ValueRange<Instant>> GetEmptyRanges(Instant start, Instant end)
    {
        if (_chunks.Count == 0)
            return Array.Empty<ValueRange<Instant>>();

        var ranges = new List<ValueRange<Instant>>();
        var from = start;

        foreach (var chunk in _chunks)
        {
            if (chunk.Range.Start >= end)
                break;

            if (chunk.Range.Start > from)
                ranges.Add(ValueRange.Create(from, chunk.Range.Start));

            from = chunk.Range.End;
        }

        if (end > from)
            ranges.Add(ValueRange.Create(from, end));

        return ranges;
    }

    public void AddData(Instant start, Instant end, IReadOnlyCollection<T> data)
    {
        var newChunk = new Chunk(start, end, data);

        if (_chunks.Count == 0)
        {
            _chunks.Add(newChunk);
            ValidateCacheIntegrity();

            return;
        }

        var isAdded = false;
        for (var i = 0; i < _chunks.Count; i++)
        {
            var chunk = _chunks[i];
            if (chunk.Range.Contains(newChunk.Range.Start, RangeBounds.Both) || chunk.Range.Contains(newChunk.Range.End, RangeBounds.Both))
                throw new InvalidOperationException($"New chunk {newChunk.Range} intersects with existing chunk {chunk}");

            if (newChunk.Range.End > chunk.Range.Start)
                continue;

            _chunks.Insert(i, newChunk);
            isAdded = true;
            break;
        }

        if (!isAdded)
            _chunks.Add(newChunk);

        Optimize();

        ValidateCacheIntegrity();
    }

    public void SetResolution(Duration resolution)
    {
        if (resolution == _resolution)
            return;

        _resolution = resolution;
        _chunks.Clear();
    }

    public void Clear()
    {
        _chunks.Clear();
    }

    private void Optimize()
    {
        var i = 0;

        while (i < _chunks.Count - 1)
        {
            var current = _chunks[i];
            var next = _chunks[i + 1];

            if (next.Range.Start - current.Range.End == _resolution)
            {
                current.Append(next);
                _chunks.RemoveAt(i + 1);
            }
            else
                i++;
        }
    }

    private void ValidateCacheIntegrity()
    {
        if (!_options.EnableIntegrityCheck)
            return;

        foreach (var chunk in _chunks)
        {
            var items = chunk.Items;
            for (var i = 1; i < items.Count; i++)
            {
                var diff = items[i].Moment - items[i - 1].Moment;
                if (diff != _resolution)
                    throw new InvalidOperationException($"Cache integrity failure: {items[i - 1]}, {items[i]}. Diff: {diff}");
            }
        }
    }

    private sealed record Chunk
    {
        public ValueRange<Instant> Range => _range;
        public List<T> Items { get; }
        private readonly ManagedValueRange<Instant> _range;

        public Chunk(Instant start, Instant end, IReadOnlyCollection<T> items)
        {
            _range = ValueRange.Create(start, end);
            Items = items.OrderBy(x => x.Moment).ToList();
            Validate();
        }

        public void Append(Chunk chunk)
        {
            _range.SetEnd(chunk.Range.End);
            Items.AddRange(chunk.Items);
        }


        private void Validate()
        {
            if (Range.Start >= Range.End)
                throw new InvalidOperationException($"Invalid chunk: {Range} is invalid");

            if (Items.Count == 0)
                return;

            if (Items[0].Moment < Range.Start)
                throw new InvalidOperationException($"Invalid chunk: {Items[0]} goes before start at {Range.Start}");

            if (Items[^1].Moment > Range.End)
                throw new InvalidOperationException($"Invalid chunk: {Items[^1]} goes after end at {Range.End}");
        }
    }
}