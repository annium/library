using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Interop;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

internal interface IManagedChartContext : IChartContext
{
    int Zoom { get; }
    int Scroll { get; }
    bool IsLocked { get; }
    void Init(Element container);
    bool ChangeZoom(decimal delta);
    bool ChangeScroll(decimal delta);
    void Adjust(Instant moment);
    void SendUpdate();
    bool TryDraw();
}