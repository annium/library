using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Domain.Contexts;

public interface IHorizontalSideContext
{
    Canvas Canvas { get; }
    Canvas Overlay { get; }
    DomRect Rect { get; }
}