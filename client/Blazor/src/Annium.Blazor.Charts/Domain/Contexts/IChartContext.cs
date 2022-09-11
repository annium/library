using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Contexts;

public interface IChartContext
{
    event Action<Instant?, Point?> LookupChanged;
    event Action Updated;
    Instant Moment { get; }
    int Zoom { get; }
    IReadOnlyList<int> Zooms { get; }
    Duration Resolution { get; }
    int PxPerResolution { get; }
    IReadOnlyList<Duration> Resolutions { get; }
    bool IsLocked { get; }
    int MsPerPx { get; }
    DateTimeZone TimeZone { get; }
    int TimeZoneOffset { get; }
    ValueRange<Instant> Bounds { get; }
    ValueRange<Instant> View { get; }
    IReadOnlyCollection<IPaneContext> Panes { get; }
    void Configure(IReadOnlyList<int> zooms, IReadOnlyList<int> resolutions);
    void SetMoment(Instant moment);
    void SetZoom(int zoom);
    void SetResolution(Duration resolution);
    Action RegisterPane(IPaneContext pane);
    void RequestDraw();
    void RequestOverlay(Point? point);
}