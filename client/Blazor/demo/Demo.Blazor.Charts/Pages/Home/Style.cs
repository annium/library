using Annium.Blazor.Css;

namespace Demo.Blazor.Charts.Pages.Home;

public class Style : RuleSet
{
    public readonly CssRule Chart = Rule.Class()
        .WidthPercent(100)
        .HeightPercent(100);

    public readonly CssRule CandlesPane = Rule.Class()
        .HeightPercent(90);

    public readonly CssRule Resolution = Rule.Class()
        .HeightPx(20)
        .RightRem(1)
        .TopRem(1);

    public readonly CssRule LinesPane = Rule.Class()
        .HeightPercent(10);

    public readonly CssRule PaneRight = Rule.Class()
        .WidthRem(4);

    public readonly CssRule PaneBottom = Rule.Class()
        .HeightRem(2);
}