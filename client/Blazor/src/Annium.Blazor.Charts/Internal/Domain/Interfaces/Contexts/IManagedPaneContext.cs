using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

/// <summary>
/// Managed pane context interface that extends IPaneContext with management capabilities
/// </summary>
internal interface IManagedPaneContext : IPaneContext
{
    /// <summary>
    /// Initializes the pane context with a chart context
    /// </summary>
    /// <param name="chart">The chart context to initialize with</param>
    void Init(IChartContext chart);

    /// <summary>
    /// Sets the rectangular bounds of the pane
    /// </summary>
    /// <param name="rect">The rectangular bounds to set</param>
    void SetRect(DomRect rect);
}
