using Annium.Blazor.Charts.Domain.Contexts;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

internal interface IManagedPaneContext : IPaneContext
{
    void Init(IChartContext chartContext);
    void SetSize(int width, int height);
}