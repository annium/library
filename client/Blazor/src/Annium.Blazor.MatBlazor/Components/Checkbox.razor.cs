using Annium.Components.State.Forms;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.MatBlazor.Components
{
    public partial class Checkbox
    {
        [Parameter]
        public IAtomicContainer<bool> State { get; set; } = default!;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public string? Class { get; set; }

        [Parameter]
        public bool Disabled { get; set; }

    }
}