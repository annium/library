namespace Annium.Blazor.Css.Internal
{
    public class JustifyContent : ImplicitString<JustifyContent>
    {
        public static readonly JustifyContent Left = new JustifyContent("left");
        public static readonly JustifyContent Right = new JustifyContent("right");
        public static readonly JustifyContent Center = new JustifyContent("center");
        public static readonly JustifyContent Start = new JustifyContent("start");
        public static readonly JustifyContent End = new JustifyContent("end");
        public static readonly JustifyContent FlexStart = new JustifyContent("flex-start");
        public static readonly JustifyContent FlexEnd = new JustifyContent("flex-end");
        public static readonly JustifyContent SpaceAround = new JustifyContent("space-around");
        public static readonly JustifyContent SpaceBetween = new JustifyContent("space-between");
        public static readonly JustifyContent SpaceEvenly = new JustifyContent("space-evenly");
        public static readonly JustifyContent Stretch = new JustifyContent("stretch");

        private JustifyContent(string type) : base(type)
        {
        }
    }
}