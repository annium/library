using System;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Interop;

public record ReferenceElement : Element
{
    public override string Id => _id.Value;
    private readonly Lazy<string> _id;

    public ReferenceElement(
        ElementReference reference
    )
    {
        _id = new Lazy<string>(() =>
        {
            var id = this.GetFullId();
            Ctx.InvokeVoid("objectTracker.track", id, reference);

            return id;
        });
    }

    public static implicit operator ReferenceElement(ElementReference reference) => new(reference);
}