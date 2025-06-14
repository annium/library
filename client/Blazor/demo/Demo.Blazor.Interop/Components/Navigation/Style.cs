using Annium.Blazor.Css;

namespace Demo.Blazor.Interop.Components.Navigation;

/// <summary>
/// CSS styles for the Navigation component
/// </summary>
public class Style : RuleSet
{
    /// <summary>
    /// Main navigation container with horizontal layout and center alignment
    /// </summary>
    public readonly CssRule Navigation = Rule.Class().FlexRow(AlignItems.Center, JustifyContent.Start);

    /// <summary>
    /// Navigation link with blue color and horizontal margin
    /// </summary>
    public readonly CssRule Link = Rule.Class().Color("blue").MarginRem(0, 1);
}
