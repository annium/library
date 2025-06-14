// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

/// <summary>
/// Provides access to common document elements for JavaScript interop.
/// </summary>
public static class Document
{
    /// <summary>
    /// Represents the document head element.
    /// </summary>
    public static readonly Element Head = new StaticElement("document.head");

    /// <summary>
    /// Represents the document body element.
    /// </summary>
    public static readonly Element Body = new StaticElement("document.body");
}
