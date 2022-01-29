using System;
using System.Collections.Generic;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Contexts;

public interface IChartContext
{
    event Action<Instant?, Point?> LookupChanged;
    event Action Updated;
    Instant Moment { get; }
    int Zoom { get; }
    bool IsLocked { get; }
    int MsPerPx { get; }
    DateTimeZone TimeZone { get; }
    ValueRange<Instant> Bounds { get; }
    ValueRange<Instant> View { get; }
    ValueRange<Instant> Range { get; }
    IReadOnlyCollection<IPaneContext> Panes { get; }
    void SetMoment(Instant moment);
    void SetZoom(int zoom);
    void RegisterPane(IPaneContext paneContext);
    void RequestDraw();
    void RequestOverlay(Point? point);
}