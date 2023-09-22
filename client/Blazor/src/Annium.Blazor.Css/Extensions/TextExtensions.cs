// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

public static class TextExtensions
{
    public static CssRule TextAlign(this CssRule rule, TextAlign align) => rule.Set("text-align", align);
}