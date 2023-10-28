using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

internal interface IManagedHorizontalSideContext : IHorizontalSideContext
{
    void Init(Canvas canvas, Canvas overlay);
    void SetRect(DomRect rect);
}
