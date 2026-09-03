using Annium.Blazor.Css;

namespace Demo.Blazor.Ant.Pages.Home;

/// <summary>
/// CSS style definitions for the Home page components.
/// </summary>
public class Style : RuleSet
{
    /// <summary>
    /// CSS rule for the main chart container with full width and height.
    /// </summary>
    public readonly CssRule Chart = Rule.Class().WidthPercent(100).HeightPercent(100);

    /// <summary>
    /// CSS rule for the candles pane with 40% height.
    /// </summary>
    public readonly CssRule CandlesPane = Rule.Class().HeightPercent(40);

    /// <summary>
    /// CSS rule for the resolution selector with fixed height and positioning.
    /// </summary>
    public readonly CssRule Resolution = Rule.Class().HeightPx(20).RightRem(1).TopRem(1);

    /// <summary>
    /// CSS rule for the lines pane with 10% height.
    /// </summary>
    public readonly CssRule LinesPane = Rule.Class().HeightPercent(10);

    /// <summary>
    /// CSS rule for the right pane with 4rem width.
    /// </summary>
    public readonly CssRule PaneRight = Rule.Class().WidthRem(4);

    /// <summary>
    /// CSS rule for the bottom pane with 2rem height.
    /// </summary>
    public readonly CssRule PaneBottom = Rule.Class().HeightRem(2);
}
