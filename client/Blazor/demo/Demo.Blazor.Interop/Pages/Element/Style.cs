using Annium.Blazor.Css;

namespace Demo.Blazor.Interop.Pages.Element;

public class Style : RuleSet
{
    public readonly CssRule Container = Rule.Class()
        .WidthPercent(100)
        .HeightPercent(100)
        .FlexColumn(AlignItems.Center, JustifyContent.Center);

    public readonly CssRule Block = Rule.Class()
        .WidthPercent(50)
        .HeightPercent(50)
        .BackgroundColor("lightblue");
}