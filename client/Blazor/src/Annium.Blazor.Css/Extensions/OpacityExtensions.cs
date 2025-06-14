using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS opacity styling.
/// </summary>
public static class OpacityExtensions
{
    /// <summary>
    /// Sets the opacity property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the opacity to.</param>
    /// <param name="opacity">The opacity value (0.0 to 1.0).</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Opacity(this CssRule rule, double opacity) => rule.Set("opacity", Invariant($"{opacity}"));
}
