using System;
using System.Collections.Generic;
using Annium.Blazor.Interop;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Contexts;

public interface IChartContext
{
    event Action Updated;
    Element Container { get; }
    DomRect Rect { get; }
    IReadOnlyCollection<IPaneContext> Panes { get; }
    int MsPerPx { get; }
    DateTimeZone TimeZone { get; }
    ValueRange<Instant> Range { get; }
    ValueRange<Instant> View { get; }
    IReadOnlyDictionary<int, LocalDateTime> VerticalLines { get; }
    void Register(IPaneContext paneContext);
    void RequestDraw();
}