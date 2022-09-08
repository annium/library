using Annium.Blazor.Interop.Internal.Extensions;

namespace Annium.Blazor.Interop;

public static partial class Window
{
    public static int GetInnerWidth() =>
        Ctx.Invoke<int>("window.innerWidth");

    public static int GetInnerHeight() =>
        Ctx.Invoke<int>("window.innerHeight");
}