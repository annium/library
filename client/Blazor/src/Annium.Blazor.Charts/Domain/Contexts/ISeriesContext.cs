using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Domain.Contexts;

public interface ISeriesContext
{
    Canvas Canvas { get; }
    Canvas Overlay { get; }
    DomRect Rect { get; }
}