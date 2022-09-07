using Annium.Blazor.Css;

namespace Demo.Blazor.Interop.Pages.Canvas;

public class Style : RuleSet
{
    public readonly CssRule Canvas = Rule.Class()
        .WidthPercent(100)
        .HeightPercent(100);
}