namespace Annium.Blazor.Interop.Internal;

/// <summary>
/// Provides helper methods for JavaScript interop operations.
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Constructs a fully qualified JavaScript function name for interop calls.
    /// </summary>
    /// <param name="name">The function name to qualify.</param>
    /// <returns>The fully qualified JavaScript function path.</returns>
    public static string Call(string name) => $"Annium.interop.{name}";
}
