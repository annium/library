namespace Annium.Blazor.Css
{
    public partial class StyleSheet
    {
        public string Styles { get; private set; } = default!;

        protected override void OnInitialized()
        {
            Styles = Sheet.ToCss();
            StateHasChanged();
        }
    }
}