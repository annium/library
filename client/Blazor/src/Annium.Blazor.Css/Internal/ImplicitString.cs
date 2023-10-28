namespace Annium.Blazor.Css.Internal;

public abstract class ImplicitString<T>
    where T : ImplicitString<T>
{
    private readonly string _type;

    protected ImplicitString(string type)
    {
        _type = type;
    }

    public override string ToString() => _type;

    public static implicit operator string(ImplicitString<T> rule) => rule.ToString();
}
