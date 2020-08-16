namespace Annium.Blazor.Css.Internal
{
    internal class RuleType
    {
        public static readonly RuleType Id = new RuleType("#");
        public static readonly RuleType Class = new RuleType(".");

        private readonly string _type;

        private RuleType(string type)
        {
            _type = type;
        }

        public override string ToString() => _type;
    }
}