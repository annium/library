using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

internal interface IManagedSeriesContext : ISeriesContext
{
    void Init(Canvas canvas, Canvas overlay);
    void SetRect(DomRect rect);
}