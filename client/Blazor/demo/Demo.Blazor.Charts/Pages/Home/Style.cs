using Annium.Blazor.Css;

namespace Demo.Blazor.Charts.Pages.Home;

/// <summary>
/// CSS styles for the Home page of the Charts demo application
/// </summary>
public class Style : RuleSet
{
    /// <summary>
    /// Main chart container with vertical layout and full dimensions
    /// </summary>
    public readonly CssRule Chart = Rule.Class()
        .FlexColumn(AlignItems.FlexStart, JustifyContent.Stretch)
        .WidthPercent(100)
        .HeightPercent(100);

    /// <summary>
    /// Pane for displaying candlestick chart data with flexible growth
    /// </summary>
    public readonly CssRule CandlesPane = Rule.Class().FlexGrow(1);

    /// <summary>
    /// Resolution selector positioned at top-right corner
    /// </summary>
    public readonly CssRule Resolution = Rule.Class().HeightPx(20).RightRem(1).TopRem(1);

    /// <summary>
    /// Zoom control positioned at bottom-right corner
    /// </summary>
    public readonly CssRule Zoom = Rule.Class().RightRem(1).BottomRem(1);

    /// <summary>
    /// Pane for displaying line chart data with 8% height
    /// </summary>
    public readonly CssRule LinesPane = Rule.Class().HeightPercent(8);

    /// <summary>
    /// Bottom pane with fixed height of 2rem
    /// </summary>
    public readonly CssRule BottomPane = Rule.Class().HeightRem(2);

    /// <summary>
    /// Right-side pane with fixed width of 4rem
    /// </summary>
    public readonly CssRule PaneRight = Rule.Class().WidthRem(4);

    /// <summary>
    /// Bottom pane with fixed height of 2rem
    /// </summary>
    public readonly CssRule PaneBottom = Rule.Class().HeightRem(2);
}
