using System.Runtime.InteropServices.JavaScript;
using static Annium.Blazor.Interop.Internal.Constants;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

public static partial class Window
{
    public static int InnerWidth => GetInnerWidth();

    [JSImport($"{JsPath}window.innerWidth")]
    private static partial int GetInnerWidth();

    public static int InnerHeight => GetInnerHeight();

    [JSImport($"{JsPath}window.innerHeight")]
    private static partial int GetInnerHeight();
}