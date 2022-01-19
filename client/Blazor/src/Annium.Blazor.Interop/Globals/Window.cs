using Annium.Blazor.Interop.Internal.Extensions;

namespace Annium.Blazor.Interop;

public static class Window
{
    private static IInteropContext Ctx => InteropContext.Instance;

    public static int GetInnerWidth() =>
        Ctx.Invoke<int>("window.innerWidth");

    public static int GetInnerHeight() =>
        Ctx.Invoke<int>("window.innerHeight");
}