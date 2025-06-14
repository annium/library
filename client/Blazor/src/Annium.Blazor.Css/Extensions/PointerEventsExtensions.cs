// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS pointer-events styling.
/// </summary>
public static class PointerEventsExtensions
{
    /// <summary>
    /// Sets the pointer-events property to 'none', making the element not a target for pointer events.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the pointer events to.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PointerEventsNone(this CssRule rule) => rule.PointerEvents("none");

    /// <summary>
    /// Sets the pointer-events property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the pointer events to.</param>
    /// <param name="events">The pointer events value.</param>
    /// <returns>The modified CSS rule.</returns>
    private static CssRule PointerEvents(this CssRule rule, string events) => rule.Set("pointer-events", events);
}
