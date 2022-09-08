namespace Annium.Blazor.Interop.Internal;

internal static class Helper
{
    public static string Call(string name) => $"Annium.interop.{name}";
}