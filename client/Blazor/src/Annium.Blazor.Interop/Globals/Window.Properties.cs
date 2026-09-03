using System.Runtime.InteropServices.JavaScript;
using static Annium.Blazor.Interop.Internal.Constants;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

/// <summary>
/// Provides access to window properties and dimensions
/// </summary>
public static partial class Window
{
    /// <summary>
    /// Gets the inner width of the window in pixels
    /// </summary>
    public static int InnerWidth => GetInnerWidth();

    /// <summary>
    /// Gets the inner width of the window from JavaScript
    /// </summary>
    /// <returns>The inner width of the window in pixels</returns>
    [JSImport($"{JsPath}window.innerWidth")]
    private static partial int GetInnerWidth();

    /// <summary>
    /// Gets the inner height of the window in pixels
    /// </summary>
    public static int InnerHeight => GetInnerHeight();

    /// <summary>
    /// Gets the inner height of the window from JavaScript
    /// </summary>
    /// <returns>The inner height of the window in pixels</returns>
    [JSImport($"{JsPath}window.innerHeight")]
    private static partial int GetInnerHeight();
}
