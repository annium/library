using System;
using System.Collections.Generic;
using Annium.Components.State.Forms;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components;

/// <summary>
/// A Blazor component that wraps a date picker input field with form field integration.
/// </summary>
public partial class DatePickerField
{
    /// <summary>
    /// The form field that manages the DateTime state and validation for this date picker.
    /// </summary>
    [CascadingParameter]
    [EditorRequired]
    public required IFormField<DateTime> FormField { get; set; }

    /// <summary>
    /// Optional external state container that overrides the form field's internal state.
    /// </summary>
    [Parameter]
    public IAtomicContainer<DateTime>? State { get; set; }

    /// <summary>
    /// The child content to be rendered inside the date picker component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to be applied to the date picker element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the internal state container, using the external state if provided, otherwise the form field's state.
    /// </summary>
    private IAtomicContainer<DateTime> InternalState => State ?? FormField.State;
}
