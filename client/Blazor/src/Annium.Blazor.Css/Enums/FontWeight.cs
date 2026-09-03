using Annium.Blazor.Css.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Represents CSS font-weight property values for controlling the thickness of font characters.
/// </summary>
public class FontWeight : ImplicitString<FontWeight>
{
    /// <summary>
    /// Extra light font weight (100).
    /// </summary>
    public static readonly FontWeight W100 = new("100");

    /// <summary>
    /// Light font weight (200).
    /// </summary>
    public static readonly FontWeight W200 = new("200");

    /// <summary>
    /// Book font weight (300).
    /// </summary>
    public static readonly FontWeight W300 = new("300");

    /// <summary>
    /// Normal font weight (400).
    /// </summary>
    public static readonly FontWeight W400 = new("400");

    /// <summary>
    /// Medium font weight (500).
    /// </summary>
    public static readonly FontWeight W500 = new("500");

    /// <summary>
    /// Semi-bold font weight (600).
    /// </summary>
    public static readonly FontWeight W600 = new("600");

    /// <summary>
    /// Bold font weight (700).
    /// </summary>
    public static readonly FontWeight W700 = new("700");

    /// <summary>
    /// Extra bold font weight (800).
    /// </summary>
    public static readonly FontWeight W800 = new("800");

    /// <summary>
    /// Black font weight (900).
    /// </summary>
    public static readonly FontWeight W900 = new("900");

    /// <summary>
    /// Initializes a new instance of the FontWeight class.
    /// </summary>
    /// <param name="type">The CSS font-weight value.</param>
    private FontWeight(string type)
        : base(type) { }
}
