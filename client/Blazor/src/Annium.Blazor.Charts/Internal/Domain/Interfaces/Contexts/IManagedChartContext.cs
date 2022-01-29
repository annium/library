using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Interop;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

internal interface IManagedChartContext : IChartContext
{
    void Init(Element container);
    void Update();
    void SetLookup(Instant? moment, Point? point);
    bool TryDraw();
    bool TryOverlay(out Point point);
}