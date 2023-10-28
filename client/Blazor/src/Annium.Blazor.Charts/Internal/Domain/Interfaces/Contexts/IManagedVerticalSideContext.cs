using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

internal interface IManagedVerticalSideContext : IVerticalSideContext
{
    void Init(Canvas canvas, Canvas overlay);
    void SetRect(DomRect rect);
}
