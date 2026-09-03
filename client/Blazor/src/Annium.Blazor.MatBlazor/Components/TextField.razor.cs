using System;
using System.Collections.Generic;
using Annium.Blazor.Core.Tools;
using Annium.Components.State.Forms;
using Annium.Core.Mapper;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.MatBlazor.Components;

/// <summary>
/// A Material Design text field component that provides state management and validation styling.
/// </summary>
/// <typeparam name="TValue">The type of value managed by the text field.</typeparam>
public partial class TextField<TValue>
    where TValue : IEquatable<TValue>
{
    /// <summary>
    /// Gets or sets the atomic container that manages the text field state.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required IAtomicContainer<TValue> State { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the text field component.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the text field element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// The injected mapper service used for type conversion during value setting.
    /// </summary>
    [Inject]
    private IMapper Mapper { get; set; } = null!;

    /// <summary>
    /// Gets the complete CSS class name for the text field, including error state styling.
    /// </summary>
    private string ClassName =>
        ClassBuilder
            .With(() => State.IsStatus(Status.Error), "mdc-text-field--invalid")
            .With(Class ?? string.Empty)
            .Build();

    /// <summary>
    /// Sets the value in the state container using the mapper for type conversion; invalid input is ignored,
    /// leaving the previous value in place (parity with the Ant text fields, and avoiding an unhandled
    /// <see cref="InvalidCastException"/> from a raw cast of the boxed input value to <typeparamref name="TValue"/>).
    /// </summary>
    /// <param name="args">The change event arguments containing the new value.</param>
    private void SetValue(ChangeEventArgs args)
    {
        try
        {
            State.Set(Mapper.Map<TValue>(args.Value!));
        }
        catch
        {
            // ignored
        }
    }
}
