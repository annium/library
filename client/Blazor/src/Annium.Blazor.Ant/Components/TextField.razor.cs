using System;
using System.Collections.Generic;
using Annium.Components.State.Forms;
using Annium.Core.Mapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Annium.Blazor.Ant.Components;

public partial class TextField<TValue>
    where TValue : IEquatable<TValue>
{
    [CascadingParameter]
    [EditorRequired]
    public required IFormField<TValue> FormField { get; set; }

    [Parameter]
    public IAtomicContainer<TValue>? State { get; set; }

    [Parameter]
    public EventCallback<KeyboardEventArgs> OnPressEnter { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; } = delegate { };

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    [Inject]
    private IMapper Mapper { get; set; } = null!;

    private TValue Value => InternalState.Value;

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

    private IAtomicContainer<TValue> InternalState => State ?? FormField.State;
}
