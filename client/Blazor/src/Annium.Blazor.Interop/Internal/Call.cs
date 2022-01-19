namespace Annium.Blazor.Interop.Internal;

internal static class Call
{
    public static string Name(string name) => $"Annium.interop.{name}";
}