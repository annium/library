// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

public static class BackgroundExtensions
{
    public static CssRule BackgroundColor(this CssRule rule, string color) => rule.Set("background-color", color);
}