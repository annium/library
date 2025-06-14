using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

/// <summary>
/// Managed series context interface that extends ISeriesContext with management capabilities
/// </summary>
internal interface IManagedSeriesContext : ISeriesContext
{
    /// <summary>
    /// Initializes the series context with canvas elements
    /// </summary>
    /// <param name="canvas">The main canvas element</param>
    /// <param name="overlay">The overlay canvas element</param>
    void Init(Canvas canvas, Canvas overlay);

    /// <summary>
    /// Sets the rectangular bounds of the series
    /// </summary>
    /// <param name="rect">The rectangular bounds to set</param>
    void SetRect(DomRect rect);
}
