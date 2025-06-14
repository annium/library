using Annium.Blazor.Css;

namespace Demo.Blazor.Interop.Pages.Element;

/// <summary>
/// Defines CSS styles for the Element demo page.
/// </summary>
public class Style : RuleSet
{
    /// <summary>
    /// CSS rule for the main container with full dimensions and centered flex layout.
    /// </summary>
    public readonly CssRule Container = Rule.Class()
        .WidthPercent(100)
        .HeightPercent(100)
        .FlexColumn(AlignItems.Center, JustifyContent.SpaceEvenly);

    /// <summary>
    /// CSS rule for the standard block element with light blue background.
    /// </summary>
    public readonly CssRule Block = Rule.Class().WidthPercent(30).HeightPercent(20).BackgroundColor("lightblue");

    /// <summary>
    /// CSS rule for the resized block element with increased height and light blue background.
    /// </summary>
    public readonly CssRule ResizedBlock = Rule.Class().WidthPercent(30).HeightPercent(30).BackgroundColor("lightblue");
}
