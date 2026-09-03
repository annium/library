using Annium.Blazor.Css;

namespace Demo.Blazor.Interop.Pages.Canvas;

/// <summary>
/// CSS styles for the Canvas page
/// </summary>
public class Style : RuleSet
{
    /// <summary>
    /// Canvas element with full width and height dimensions
    /// </summary>
    public readonly CssRule Canvas = Rule.Class().WidthPercent(100).HeightPercent(100);
}
