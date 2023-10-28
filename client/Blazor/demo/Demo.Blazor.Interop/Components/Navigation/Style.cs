using Annium.Blazor.Css;

namespace Demo.Blazor.Interop.Components.Navigation;

public class Style : RuleSet
{
    public readonly CssRule Navigation = Rule.Class().FlexRow(AlignItems.Center, JustifyContent.Start);

    public readonly CssRule Link = Rule.Class().Color("blue").MarginRem(0, 1);
}
