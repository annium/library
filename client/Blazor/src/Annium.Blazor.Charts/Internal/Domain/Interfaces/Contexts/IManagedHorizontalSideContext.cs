using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;

/// <summary>
/// Managed horizontal side context interface that extends IHorizontalSideContext with management capabilities
/// </summary>
internal interface IManagedHorizontalSideContext : IHorizontalSideContext
{
    /// <summary>
    /// Initializes the horizontal side context with canvas elements
    /// </summary>
    /// <param name="canvas">The main canvas element</param>
    /// <param name="overlay">The overlay canvas element</param>
    void Init(Canvas canvas, Canvas overlay);

    /// <summary>
    /// Sets the rectangular bounds of the horizontal side
    /// </summary>
    /// <param name="rect">The rectangular bounds to set</param>
    void SetRect(DomRect rect);
}
