using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Interop;
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
    Element Container { get; }
    DomRect Rect { get; }
    IReadOnlyCollection<IPaneContext> Panes { get; }
    int MsPerPx { get; }
    DateTimeZone TimeZone { get; }
    ValueRange<Instant> Range { get; }
    ValueRange<Instant> View { get; }
    IReadOnlyDictionary<int, LocalDateTime> VerticalLines { get; }
    void Adjust(Instant moment);
    bool ChangeScroll(decimal delta);
    void SetZoom(int zoom);
    void RegisterPane(IPaneContext paneContext);
    void RegisterSource(ISeriesSource source);
    void RequestDraw();
    void RequestOverlay(Point? point);
}