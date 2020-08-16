using System;

namespace Annium.Blazor.Css
{
    public interface IRule
    {
        IRule Set(string property, string value);
        IRule And(string selector, Action<IRule> configure);
        IRule Child(string selector, Action<IRule> configure);
        IRule Inheritor(string selector, Action<IRule> configure);
        string ToCss();
    }
}