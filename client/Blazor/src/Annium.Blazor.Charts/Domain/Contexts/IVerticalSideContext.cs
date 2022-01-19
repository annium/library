using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Domain.Contexts;

public interface IVerticalSideContext
{
    Canvas Canvas { get; }
    Canvas Overlay { get; }
    DomRect Rect { get; }
}