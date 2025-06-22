using System.Collections.Generic;
using Annium.Components.State.Forms;
using Annium.Core.Mapper;
using AntDesign;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components;

/// <summary>
/// A Blazor component that wraps a password input field with form field integration and keyboard event handling.
/// </summary>
public partial class PasswordField
{
    /// <summary>
    /// The form field that manages the string state and validation for this password field.
    /// </summary>
    [CascadingParameter]
    [EditorRequired]
    public required IFormField<string> FormField { get; set; }

    /// <summary>
    /// Optional external state container that overrides the form field's internal state.
    /// </summary>
    [Parameter]
    public IAtomicContainer<string>? State { get; set; }

    /// <summary>
    /// Event callback triggered when the Enter key is pressed in the password field.
    /// </summary>
    [Parameter]
    public EventCallback<PressEnterEventArgs> OnPressEnter { get; set; }

    /// <summary>
    /// The child content to be rendered inside the password field component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = delegate { };

    /// <summary>
    /// Additional HTML attributes to be applied to the password input element.
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
    private string Value => InternalState.Value;

    /// <summary>
    /// Sets the value in the internal state container using the mapper for type conversion.
    /// </summary>
    /// <param name="args">The change event arguments containing the new value.</param>
    private void SetValue(ChangeEventArgs args)
    {
        try
        {
            InternalState.Set(Mapper.Map<string>(args.Value!));
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Gets the internal state container, using the external state if provided, otherwise the form field's state.
    /// </summary>
    private IAtomicContainer<string> InternalState => State ?? FormField.State;
}
