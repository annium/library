namespace Annium.Blazor.Css.Internal
{
    internal class RuleType : ImplicitString<RuleType>
    {
        public static readonly RuleType Id = new RuleType("#");
        public static readonly RuleType Class = new RuleType(".");

        private RuleType(string type) : base(type)
        {
        }
    }
}