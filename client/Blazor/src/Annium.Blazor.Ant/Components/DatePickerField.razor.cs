using System;
using System.Collections.Generic;
using Annium.Components.State.Forms;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components;

public partial class DatePickerField
{
    [CascadingParameter]
    [EditorRequired]
    public required IFormField<DateTime> FormField { get; set; }

    [Parameter]
    public IAtomicContainer<DateTime>? State { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    private IAtomicContainer<DateTime> InternalState => State ?? FormField.State;
}
