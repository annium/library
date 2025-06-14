using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Interop;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

/// <summary>
/// Managed chart context interface that extends IChartContext with management capabilities
/// </summary>
internal interface IManagedChartContext : IChartContext
{
    /// <summary>
    /// Gets the rectangular bounds of the chart
    /// </summary>
    DomRect Rect { get; }

    /// <summary>
    /// Sets the rectangular bounds of the chart
    /// </summary>
    /// <param name="rect">The rectangular bounds to set</param>
    void SetRect(DomRect rect);

    /// <summary>
    /// Updates the chart context
    /// </summary>
    void Update();

    /// <summary>
    /// Sets the lookup information for the chart
    /// </summary>
    /// <param name="moment">The instant moment for lookup</param>
    /// <param name="point">The point for lookup</param>
    void SetLookup(Instant? moment, Point? point);

    /// <summary>
    /// Attempts to draw the chart
    /// </summary>
    /// <returns>True if drawing was successful, false otherwise</returns>
    bool TryDraw();

    /// <summary>
    /// Attempts to get the overlay point
    /// </summary>
    /// <param name="point">The overlay point if successful</param>
    /// <returns>True if overlay point was retrieved successfully, false otherwise</returns>
    bool TryOverlay(out Point point);
}
