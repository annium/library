using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css
{
    public class FlexDirection : ImplicitString<FlexDirection>
    {
        public static readonly FlexDirection Row = new FlexDirection("row");
        public static readonly FlexDirection Column = new FlexDirection("column");
        public static readonly FlexDirection RowInverse = new FlexDirection("row-inverse");
        public static readonly FlexDirection ColumnInverse = new FlexDirection("column-inverse");

        private FlexDirection(string type) : base(type)
        {
        }
    }
}