using System.Collections.Generic;
using Annium.Components.State.Forms;
using Annium.Core.Mapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Annium.Blazor.Ant.Components;

public partial class PasswordField
{
    [CascadingParameter]
    [EditorRequired]
    public required IFormField<string> FormField { get; set; }

    [Parameter]
    public IAtomicContainer<string>? State { get; set; }

    [Parameter]
    public EventCallback<KeyboardEventArgs> OnPressEnter { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; } = delegate { };

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    [Inject]
    private IMapper Mapper { get; set; } = default!;

    private string Value => InternalState.Value;

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

    private IAtomicContainer<string> InternalState => State ?? FormField.State;
}