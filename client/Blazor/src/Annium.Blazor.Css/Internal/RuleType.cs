namespace Annium.Blazor.Css.Internal;

internal class RuleType : ImplicitString<RuleType>
{
    public static readonly RuleType Id = new("#");
    public static readonly RuleType Class = new(".");

    private RuleType(string type)
        : base(type) { }
}
