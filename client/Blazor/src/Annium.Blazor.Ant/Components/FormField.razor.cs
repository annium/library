using System;
using Annium.Blazor.Core.Tools;
using Annium.Components.State.Forms;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components;

/// <summary>
/// Represents a form field component that provides Ant Design styling and state management for form inputs.
/// </summary>
/// <typeparam name="TValue">The type of value contained in the form field state.</typeparam>
public partial class FormField<TValue> : IFormField<TValue>
    where TValue : IEquatable<TValue>
{
    /// <summary>
    /// Gets or sets the atomic container that manages the state of the form field.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required IAtomicContainer<TValue> State { get; set; }

    /// <summary>
    /// Gets or sets the child content to render within the form field.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = delegate { };

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the form field.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the computed CSS class name for the form field, including Ant Design classes and error states.
    /// </summary>
    private string ClassName =>
        ClassBuilder
            .With("ant-form-item")
            .With(() => State.HasBeenTouched && State.HasStatus(Status.Error), "ant-form-item-has-error")
            .With(Class ?? string.Empty)
            .Build();
}
