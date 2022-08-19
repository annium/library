namespace Annium.Blazor.Interop;

public static class Document
{
    public static readonly Element Head = new StaticElement("document.head");
    public static readonly Element Body = new StaticElement("document.body");
}