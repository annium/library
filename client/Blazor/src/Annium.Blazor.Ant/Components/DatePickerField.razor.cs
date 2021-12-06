using System;
using System.Collections.Generic;
using Annium.Components.State.Forms;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components;

public partial class DatePickerField
{
    [CascadingParameter]
    public IFormField<DateTime> FormField { get; set; } = default!;

    [Parameter]
    public IAtomicContainer<DateTime> State { get; set; } = default!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }


    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    private IAtomicContainer<DateTime> InternalState => State ?? FormField.State;
}