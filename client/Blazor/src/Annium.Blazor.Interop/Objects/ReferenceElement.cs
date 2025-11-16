using System;
using Annium.Blazor.Interop.Internal.Extensions;
using Microsoft.AspNetCore.Components;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

/// <summary>
/// Represents a DOM element that is referenced through a Blazor ElementReference
/// </summary>
public record ReferenceElement : Element
{
    /// <summary>
    /// Gets the unique identifier of the element
    /// </summary>
    public override string Id => _id.Value;

    /// <summary>
    /// Lazy-loaded unique identifier for the element
    /// </summary>
    private readonly Lazy<string> _id;

    /// <summary>
    /// Initializes a new instance of the ReferenceElement class
    /// </summary>
    /// <param name="reference">The Blazor element reference</param>
    public ReferenceElement(ElementReference reference)
    {
        _id = new Lazy<string>(() =>
        {
            var id = this.GetFullId();
            Ctx.CallHelper("objectTracker.track", id, reference);

            return id;
        });
    }

    /// <summary>
    /// Implicitly converts an ElementReference to a ReferenceElement instance
    /// </summary>
    /// <param name="reference">The element reference to convert</param>
    /// <returns>A new ReferenceElement instance</returns>
    public static implicit operator ReferenceElement(ElementReference reference) => new(reference);
}
