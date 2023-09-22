// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

public static class PointerEventsExtensions
{
    public static CssRule PointerEventsNone(this CssRule rule) =>
        rule.PointerEvents("none");

    private static CssRule PointerEvents(this CssRule rule, string events) =>
        rule.Set("pointer-events", events);
}