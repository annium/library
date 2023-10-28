using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Interop;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

internal interface IManagedChartContext : IChartContext
{
    DomRect Rect { get; }
    void SetRect(DomRect rect);
    void Update();
    void SetLookup(Instant? moment, Point? point);
    bool TryDraw();
    bool TryOverlay(out Point point);
}
