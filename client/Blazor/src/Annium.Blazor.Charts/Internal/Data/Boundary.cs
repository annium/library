using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Data.Models;
using Annium.Logging.Abstractions;
using Annium.NodaTime.Extensions;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Internal.Data;

internal class Boundary : ILogSubject<Boundary>
{
    public ILogger<Boundary> Logger { get; }
    public ValueRange<Instant> Bounds { get; }
    public bool HasDefaultState => _emptyRange.Start == _emptyRange.End;
    private readonly ITimeProvider _timeProvider;
    private readonly ManagedValueRange<Instant> _emptyBefore;
    private readonly ManagedValueRange<Instant> _emptyRange;
    private readonly ManagedValueRange<Instant> _emptyAfter;

    public Boundary(
        ITimeProvider timeProvider,
        ILogger<Boundary> logger
    )
    {
        _timeProvider = timeProvider;
        Logger = logger;

        var now = timeProvider.Now.FloorToMinute();
        _emptyBefore = ValueRange.Create(now, now);
        _emptyRange = ValueRange.Create(now, now);
        _emptyAfter = ValueRange.Create(now, now);

        Reset();

        Bounds = ValueRange.Create(() => _emptyBefore.End, () => _emptyAfter.Start);
    }

    public (Instant, Instant) GetBounds(Instant start, Instant end, long zone)
    {
        var size = Duration.FromTicks((end - start).TotalTicks.FloorInt64() * zone);

        var min = Instant.Max(start - size, Bounds.Start);
        var max = Instant.Min(end + size, Bounds.End);

        return (min, max);
    }

    public IReadOnlyCollection<ValueRange<Instant>> GetUnprocessedRanges(ValueRange<Instant> range) =>
        range - _emptyRange;

    public void ExtendBounds(Instant start, Instant end)
    {
        if (start > _emptyBefore.End)
        {
            this.Log().Trace($"data loaded from start side, update emptyBefore.End: {S(_emptyBefore.End)} -> {S(start)}");
            _emptyBefore.SetEnd(start);
        }

        if (end < _emptyAfter.Start)
        {
            this.Log().Trace($"data loaded from end side, update emptyAfter.Start: {S(_emptyAfter.Start)} -> {S(end)}");
            _emptyAfter.SetStart(end);
        }
    }

    public void ShrinkBounds(Instant start, Instant end)
    {
        if (start < _emptyRange.Start)
        {
            this.Log().Trace($"empty cache, update emptyRange.Start: {S(_emptyRange.Start)} -> {S(start)}");
            _emptyRange.SetStart(start);
        }

        if (end > _emptyRange.End)
        {
            this.Log().Trace($"empty cache, update emptyRange.End: {S(_emptyRange.End)} -> {S(end)}");
            _emptyRange.SetEnd(end);
        }
    }

    public bool Contains(Instant from, Instant to) =>
        _emptyRange.Contains(from, RangeBounds.Both) && _emptyRange.Contains(to, RangeBounds.Both);

    public void Reset()
    {
        var now = _timeProvider.Now.FloorToMinute();
        _emptyBefore.SetStart(Instant.MinValue);
        _emptyBefore.SetEnd(now - Duration.FromDays(10000));
        _emptyRange.SetStart(now);
        _emptyRange.SetEnd(now);
        _emptyAfter.SetStart(now);
        _emptyAfter.SetEnd(Instant.MaxValue);
    }
}