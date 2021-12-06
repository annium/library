using System;
using Annium.Blazor.Core.Tools;
using Annium.Components.State.Forms;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components;

public partial class FormField<TValue> : IFormField<TValue>
    where TValue : IEquatable<TValue>
{
    [Parameter]
    public IAtomicContainer<TValue> State { get; set; } = default!;

    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    [Parameter]
    public string? Class { get; set; }

    private string ClassName => ClassBuilder
        .With("ant-form-item")
        .With(() => State.HasBeenTouched && State.HasStatus(Status.Error), "ant-form-item-has-error")
        .With(Class ?? string.Empty)
        .Build();
}