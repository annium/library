using Annium.Blazor.Css;

namespace Demo.Blazor.Interop.Pages.Element;

public class Style : RuleSet
{
    public readonly CssRule Container = Rule.Class()
        .WidthPercent(100)
        .HeightPercent(100)
        .FlexColumn(AlignItems.Center, JustifyContent.SpaceEvenly);

    public readonly CssRule Block = Rule.Class()
        .WidthPercent(30)
        .HeightPercent(20)
        .BackgroundColor("lightblue");

    public readonly CssRule ResizedBlock = Rule.Class()
        .WidthPercent(30)
        .HeightPercent(30)
        .BackgroundColor("lightblue");
}