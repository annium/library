using Annium.Blazor.Interop;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Internal.Extensions;

/// <summary>
/// Canvas drawing helpers shared across chart pane components.
/// </summary>
internal static class CanvasExtensions
{
    /// <summary>
    /// Strokes the rectangular pane boundary (inset by half a grid line) on the canvas using the shared grid style.
    /// </summary>
    /// <param name="ctx">The canvas to draw on.</param>
    /// <param name="width">The pane width in pixels.</param>
    /// <param name="height">The pane height in pixels.</param>
    public static void DrawBoundaries(this Canvas ctx, int width, int height)
    {
        ctx.StrokeStyle = GridStyle;
        ctx.LineWidth = GridLine;

        ctx.BeginPath();
        ctx.MoveTo(GridHalfLine, GridHalfLine);
        ctx.LineTo(width - GridHalfLine, GridHalfLine);
        ctx.LineTo(width - GridHalfLine, height - GridHalfLine);
        ctx.LineTo(GridHalfLine, height - GridHalfLine);
        ctx.LineTo(GridHalfLine, GridHalfLine);
        ctx.Stroke();
        ctx.ClosePath();
    }
}
