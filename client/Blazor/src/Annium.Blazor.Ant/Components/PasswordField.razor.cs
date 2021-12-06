using System.Collections.Generic;
using Annium.Components.State.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Annium.Blazor.Ant.Components;

public partial class PasswordField
{
    [CascadingParameter]
    public IFormField<string> FormField { get; set; } = default!;

    [Parameter]
    public IAtomicContainer<string> State { get; set; } = default!;

    [Parameter]
    public EventCallback<KeyboardEventArgs> OnPressEnter { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = default!;

    private string Value => InternalState.Value;

    private void SetValue(ChangeEventArgs args)
    {
        try
        {
            InternalState.Set(mapper.Map<string>(args.Value!));
        }
        catch
        {
        }
    }

    private IAtomicContainer<string> InternalState => State ?? FormField.State;
}