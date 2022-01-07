using System;

namespace Annium.Blazor.Css;

public abstract class CssTopLevelRule : CssRule
{
    public abstract CssTopLevelRule Media(string query, Action<CssRule> configure);
}

public abstract class CssRule
{
    public string Name { get; protected set; } = string.Empty;
    public abstract CssRule Set(string property, string value);
    public abstract CssRule And(string selector, Action<CssRule> configure);
    public abstract CssRule Child(string selector, Action<CssRule> configure);
    public abstract CssRule Inheritor(string selector, Action<CssRule> configure);
    public abstract string Inline();
    public abstract string ToCss();
    public static implicit operator string(CssRule rule) => rule.Name;
}