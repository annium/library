using Annium.Blazor.Css.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

public class FontWeight : ImplicitString<FontWeight>
{
    public static readonly FontWeight W100 = new("100");
    public static readonly FontWeight W200 = new("200");
    public static readonly FontWeight W300 = new("300");
    public static readonly FontWeight W400 = new("400");
    public static readonly FontWeight W500 = new("500");
    public static readonly FontWeight W600 = new("600");
    public static readonly FontWeight W700 = new("700");
    public static readonly FontWeight W800 = new("800");
    public static readonly FontWeight W900 = new("900");

    private FontWeight(string type) : base(type)
    {
    }
}