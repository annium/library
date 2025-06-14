using System;
using System.Collections.Generic;
using Annium.Components.State.Forms;
using Annium.Core.Mapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Annium.Blazor.Ant.Components;

/// <summary>
/// A generic Blazor component that wraps a text input field with form field integration and keyboard event handling.
/// </summary>
/// <typeparam name="TValue">The type of value that the text field manages, must implement IEquatable.</typeparam>
public partial class TextField<TValue>
    where TValue : IEquatable<TValue>
{
    /// <summary>
    /// The form field that manages the typed state and validation for this text field.
    /// </summary>
    [CascadingParameter]
    [EditorRequired]
    public required IFormField<TValue> FormField { get; set; }

    /// <summary>
    /// Optional external state container that overrides the form field's internal state.
    /// </summary>
    [Parameter]
    public IAtomicContainer<TValue>? State { get; set; }

    /// <summary>
    /// Event callback triggered when the Enter key is pressed in the text field.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnPressEnter { get; set; }

    /// <summary>
    /// The child content to be rendered inside the text field component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = delegate { };

    /// <summary>
    /// Additional HTML attributes to be applied to the text input element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// The injected mapper service used for type conversion during value setting.
    /// </summary>
    [Inject]
    private IMapper Mapper { get; set; } = null!;

    /// <summary>
    /// Gets the current value from the internal state container.
    /// </summary>
    private TValue Value => InternalState.Value;

    /// <summary>
    /// Sets the value in the internal state container using the mapper for type conversion.
    /// </summary>
    /// <param name="args">The change event arguments containing the new value.</param>
    private void SetValue(ChangeEventArgs args)
    {
        try
        {
            InternalState.Set(Mapper.Map<TValue>(args.Value!));
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Gets the internal state container, using the external state if provided, otherwise the form field's state.
    /// </summary>
    private IAtomicContainer<TValue> InternalState => State ?? FormField.State;
}
