using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

internal sealed record SeriesContext : IManagedSeriesContext
{
    public Canvas Canvas { get; private set; } = default!;
    public Canvas Overlay { get; private set; } = default!;
    public DomRect Rect { get; private set; }

    public void Init(
        Canvas canvas,
        Canvas overlay,
        DomRect rect
    )
    {
        Canvas = canvas;
        Overlay = overlay;
        Rect = rect;
    }
}