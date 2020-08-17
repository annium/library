using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css
{
    public class AlignItems : ImplicitString<AlignItems>
    {
        public static readonly AlignItems Baseline = new AlignItems("baseline");
        public static readonly AlignItems Center = new AlignItems("center");
        public static readonly AlignItems Start = new AlignItems("start");
        public static readonly AlignItems End = new AlignItems("end");
        public static readonly AlignItems FlexStart = new AlignItems("flex-start");
        public static readonly AlignItems FlexEnd = new AlignItems("flex-end");
        public static readonly AlignItems SelfStart = new AlignItems("self-start");
        public static readonly AlignItems SelfEnd = new AlignItems("self-end");
        public static readonly AlignItems Revert = new AlignItems("revert");
        public static readonly AlignItems Stretch = new AlignItems("stretch");

        private AlignItems(string type) : base(type)
        {
        }
    }
}