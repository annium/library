using System.Collections.Generic;
using Annium.Components.State.Forms;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components;

public partial class CheckboxField
{
    [CascadingParameter]
    [EditorRequired]
    public required IFormField<bool> FormField { get; set; }

    [Parameter]
    public IAtomicContainer<bool>? State { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    private IAtomicContainer<bool> InternalState => State ?? FormField.State;
}