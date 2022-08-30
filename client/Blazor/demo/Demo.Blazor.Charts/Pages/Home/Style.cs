using Annium.Blazor.Css;

namespace Demo.Blazor.Charts.Pages.Home;

public class Style : RuleSet
{
    public readonly CssRule Text = Rule.Class()
        .FontSizeRem(2);
}