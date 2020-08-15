using System.Collections.Generic;
using Annium.Components.State;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components
{
    public partial class CheckboxField
    {
        [Parameter]
        public IAtomicContainer<bool> State { get; set; } = default!;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = default!;
    }
}