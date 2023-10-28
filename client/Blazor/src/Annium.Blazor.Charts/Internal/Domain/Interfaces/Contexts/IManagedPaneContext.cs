using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

internal interface IManagedPaneContext : IPaneContext
{
    void Init(IChartContext chart);
    void SetRect(DomRect rect);
}
